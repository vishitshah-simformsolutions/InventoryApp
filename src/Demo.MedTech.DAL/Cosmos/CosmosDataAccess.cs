using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Demo.MedTech.DataModel.Exceptions;
using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.Utility.Helper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Demo.MedTech.DAL.Cosmos
{
    public class CosmosDataAccess : ILotDataAccess
    {
        private const int RetryMaxAttempts = 2;
        private static readonly TimeSpan CumulativeRetryTime = TimeSpan.FromSeconds(1);
        private Container _biddingContainer;
        private readonly ICompressHelper _compressHelper;

        protected virtual IDisposable ChangeListener { get; }

        public CosmosDataAccess(IOptionsMonitor<SbsConfigurationOptions> optionsMonitor, ICompressHelper compressHelper)
        {
            CreateCosmosClient(optionsMonitor.CurrentValue);
            ChangeListener = optionsMonitor.OnChange(OptionChanged);
            _compressHelper = compressHelper;
        }

        /// <summary>
        /// Configuration change event.
        /// </summary>
        /// <param name="configurationOptions">Latest configuration.</param>
        private void OptionChanged(SbsConfigurationOptions configurationOptions)
        {
            CreateCosmosClient(configurationOptions);
        }

        private void CreateCosmosClient(SbsConfigurationOptions configurationOptions)
        {
            var endpointUri = configurationOptions.CosmosDbEndpoint;
            var primaryKey = configurationOptions.CosmosDbMasterKey;
            var databaseId = configurationOptions.CosmosDatabase;
            var containerName = configurationOptions.CosmosDdContainerName;

            if (string.IsNullOrWhiteSpace(endpointUri) || string.IsNullOrWhiteSpace(primaryKey) ||
                string.IsNullOrWhiteSpace(databaseId) || string.IsNullOrWhiteSpace(containerName))
            {
                throw new MissingFieldException("Missing cosmos connection properties.");
            }

            var cosmosClient = new CosmosClient(endpointUri, primaryKey, new CosmosClientOptions
            {
                MaxRetryAttemptsOnRateLimitedRequests = RetryMaxAttempts,
                MaxRetryWaitTimeOnRateLimitedRequests = CumulativeRetryTime,
                ConsistencyLevel = ConsistencyLevel.Session,
                RequestTimeout = TimeSpan.FromMilliseconds(500),
                AllowBulkExecution = true
            });
            var database = cosmosClient.GetDatabase(databaseId);
            _biddingContainer = database.GetContainer(containerName);
        }

        /// <summary>
        /// Retrieves the lot model from Cosmos DB based on auction id and lot id
        /// </summary>
        /// <param name="auctionId">Auction id</param>
        /// <param name="lotId">Lot id</param>
        /// <returns>Retrieved lot model</returns>
        public async Task<LotModel> GetAsync(long auctionId, long lotId)
        {
            try
            {
                var lotModel = new LotModel();
                var cosmosItemResponse = await _biddingContainer.ReadItemAsync<CosmosLotDocument>(lotId.ToString(), new PartitionKey(string.Concat(auctionId, "-", lotId)), cancellationToken: CancellationToken.None);
                lotModel = JsonSerializer.Deserialize<LotModel>(await _compressHelper.Decompress(cosmosItemResponse.Resource.EncodedLotModel), JsonSerializerOption.CaseInsensitive);
                lotModel.ETag = cosmosItemResponse.ETag;

                return lotModel;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(105) };
                    Dictionary<string, object> dictionarySuggestion = new Dictionary<string, object>
                    {
                        {"auctionId",auctionId},
                        {"lotId",lotId},
                    };
                    validationResult[0].Data = dictionarySuggestion;
                    throw new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Retrieves the lot model from Cosmos DB based on document id auction id and lot id
        /// </summary>
        /// <param name="documentId">Document Id</param>
        /// <param name="auctionId">Auction id</param>
        /// <param name="lotId">Lot id</param>
        /// <returns>Retrieved lot model</returns>
        public async Task<LotModel> GetByIdAsync(string documentId, long auctionId, long lotId)
        {
            try
            {
                var lotModel = new LotModel();
                var cosmosItemResponse = await _biddingContainer.ReadItemAsync<CosmosLotDocument>(documentId, new PartitionKey(string.Concat(auctionId, "-", lotId)), cancellationToken: CancellationToken.None);
                lotModel = JsonSerializer.Deserialize<LotModel>(await _compressHelper.Decompress(cosmosItemResponse.Resource.EncodedLotModel), JsonSerializerOption.CaseInsensitive);
                lotModel.ETag = cosmosItemResponse.ETag;

                return lotModel;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(105) };
                    Dictionary<string, object> dictionarySuggestion = new Dictionary<string, object>
                    {
                        {"auctionId",auctionId},
                        {"lotId",lotId},
                    };
                    validationResult[0].Data = dictionarySuggestion;
                    throw new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Creates a new document in cosmos DB based on provided lot model
        /// </summary>
        /// <param name="lotModel">Lot Model</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public async Task CreateAsync(LotModel lotModel, CancellationToken cancellationToken)
        {
            var document = new CosmosLotDocument
            {
                id = lotModel.LotDetail.LotId.ToString(),
                PartitionKey = string.Concat(lotModel.LotDetail.AuctionId, "-", lotModel.LotDetail.LotId),
                EncodedLotModel = await _compressHelper.Compress(JsonSerializer.Serialize(lotModel, JsonSerializerOption.CamelCasePolicyWithEnumAndDateTimeConverter))
            };

            ItemRequestOptions requestOptions = new ItemRequestOptions { EnableContentResponseOnWrite = false };
            try
            {
                await _biddingContainer.CreateItemAsync(document, new PartitionKey(document.PartitionKey), requestOptions, cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(164) };
                    Dictionary<string, object> dictionarySuggestion = new Dictionary<string, object>
                    {
                        {"auctionId",lotModel.LotDetail.AuctionId},
                        {"lotId",lotModel.LotDetail.LotId},
                    };
                    validationResult[0].Data = dictionarySuggestion;
                    throw new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Deletes the lot model from Cosmos DB based on auction id and lot id
        /// </summary>
        /// <param name="auctionId">Auction id</param>
        /// <param name="lotId">Lot id</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public async Task DeleteAsync(long auctionId, long lotId, CancellationToken cancellationToken)
        {
            try
            {
                await _biddingContainer.DeleteItemAsync<CosmosLotDocument>(lotId.ToString(), new PartitionKey(string.Concat(auctionId, "-", lotId)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(105) };
                    Dictionary<string, object> dictionarySuggestion = new Dictionary<string, object>
                    {
                        {"auctionId",auctionId},
                        {"lotId",lotId},
                    };
                    validationResult[0].Data = dictionarySuggestion;
                    throw new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Deletes the lot model from Cosmos DB based on auction id and lot id
        /// </summary>
        /// <param name="id">Cosmos document id</param>
        /// <param name="auctionId">Auction id</param>
        /// <param name="lotId">Lot id</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="ignoreNotFoundException">Ignoring the exception for HttpStatusCode.NotFound</param>
        public async Task DeleteAsync(string id, long auctionId, long lotId, CancellationToken cancellationToken, bool ignoreNotFoundException = false)
        {
            try
            {
                await _biddingContainer.DeleteItemAsync<CosmosLotDocument>(id, new PartitionKey(string.Concat(auctionId, "-", lotId)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (!ignoreNotFoundException && ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(105) };
                    Dictionary<string, object> dictionarySuggestion = new Dictionary<string, object>
                    {
                        {"auctionId",auctionId},
                        {"lotId",lotId},
                    };
                    validationResult[0].Data = dictionarySuggestion;
                    throw new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, ex);
                }

                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// Deletes the lot model from Cosmos DB based on auction id and lot id
        /// </summary>
        /// <param name="auctionId">Auction id</param>
        /// <param name="lotId">Lot id</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public async Task DeleteByPartitionKeyAsync(long auctionId, long lotId, CancellationToken cancellationToken)
        {
            const string storedProcedureId = "bulkDeleteSproc";
            try
            {
                //{DON'T REMOVE}
                //Temporary code to create cosmos stored procedure
                //Create Stored Procedure
                //var storedProcedureResponse = await _biddingContainer.Scripts.CreateStoredProcedureAsync(new StoredProcedureProperties
                //{
                //    Id = storedProcedureId,
                //    Body = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Cosmos\\BulkDelete.js"), cancellationToken),
                //}, cancellationToken: cancellationToken);

                //Execute Stored Procedure
                ResponseBody responseBody;
                do
                {
                    var sprocResponse = await _biddingContainer.Scripts.ExecuteStoredProcedureAsync<ResponseBody>(
                        storedProcedureId, new PartitionKey(string.Concat(auctionId, "-", lotId)),
                        new string[] { "SELECT c._self FROM c WHERE c.PartitionKey = '" + string.Concat(auctionId, "-", lotId) + "'" }, cancellationToken: cancellationToken);
                    responseBody = sprocResponse.Resource;
                } while (responseBody.Continuation);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(105) };
                    var dictionarySuggestion = new Dictionary<string, object>
                    {
                        {"auctionId",auctionId},
                        {"lotId",lotId},
                    };
                    validationResult[0].Data = dictionarySuggestion;
                    throw new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Update and Insert lot document
        /// </summary>
        /// <param name="lotModel"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="isRetraction"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public async Task<string> UpsertAsync(LotModel lotModel, CancellationToken cancellationToken, bool isRetraction = false, string documentId = "")
        {
            string id = isRetraction ? documentId : lotModel.LotDetail.LotId.ToString();

            var document = new CosmosLotDocument
            {
                id = id,
                PartitionKey = $"{lotModel.LotDetail.AuctionId}-{lotModel.LotDetail.LotId}",
                EncodedLotModel = await _compressHelper.Compress(JsonSerializer.Serialize(lotModel, JsonSerializerOption.CamelCasePolicyWithEnumAndDateTimeConverter))
            };

            ItemRequestOptions requestOptions = new ItemRequestOptions { IfMatchEtag = lotModel.ETag, EnableContentResponseOnWrite = false };
            string updatedETag = string.Empty;
            var cosmosResponse = await _biddingContainer.UpsertItemAsync(document, new PartitionKey(document.PartitionKey), requestOptions, cancellationToken);
            updatedETag = cosmosResponse.ETag;

            return updatedETag;
        }

        private class ResponseBody
        {
            public int Deleted { get; set; }

            public bool Continuation { get; set; }
        }

        ~CosmosDataAccess()
        {
            ChangeListener?.Dispose();
        }
    }
}
