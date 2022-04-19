using Product.DAL;
using Product.DataModel.Request;
using Product.DataModel.Response;
using Product.DataModel.Shared;
using Product.Utility;
using Product.Utility.Helper;
using Product.ValidationEngine.Model;
using Product.ValidationEngine.Rules;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Service
{
    public class AuctioneerService : IAuctioneerService
    {
        private readonly IEnumerable<IRule> _rules;
        private readonly IEnumerable<ITransform> _transformationRules;
        private readonly ILotDataAccess _lotDataAccess;
        private readonly IRequestPipe _requestPipe;

        public AuctioneerService
            (
                ILotDataAccess lotDataAccess,
                IRequestPipe requestPipe,
                IEnumerable<IRule> rules,
                IEnumerable<ITransform> transformationRules
            )
        {
            _lotDataAccess = lotDataAccess;
            _requestPipe = requestPipe;
            _rules = rules;
            _transformationRules = transformationRules;
        }

        #region Ingress

        public async Task<EditedLotResponse> InsertWithRetryAsync(string request, CancellationToken cancellationToken)
        {
            return await CreateAsync(request, cancellationToken);
        }

        public async Task DeleteWithRetryAsync(long AuctionId, long LotId, CancellationToken cancellationToken)
        {
            await DeleteAsync(AuctionId, LotId, cancellationToken);
        }

        public async Task DeleteByPartitionKeyWithRetryAsync(long AuctionId, long LotId, CancellationToken cancellationToken)
        {
            await DeleteByPartitionKeyAsync(AuctionId, LotId, cancellationToken);
        }

        public async Task<EditedLotResponse> GetAsync(long AuctionId, long LotId)
        {
            var lotModel = await _lotDataAccess.GetAsync(AuctionId, LotId);
            return new EditedLotResponse
            {
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                LotDetail = lotModel.LotDetail,
                BiddingStates = lotModel.BiddingStates,
            };
        }

        public async Task<EditedLotResponse> GetByIdAsync(string documentId, long AuctionId, long LotId)
        {
            var lotModel = await _lotDataAccess.GetByIdAsync(documentId, AuctionId, LotId);
            return new EditedLotResponse
            {
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                LotDetail = lotModel.LotDetail,
                BiddingStates = lotModel.BiddingStates,
            };
        }



        private async Task<EditedLotResponse> CreateAsync(string request, CancellationToken cancellationToken)
        {
            ProductModel lotModel = new ProductModel();
            RuleValidationMessage ruleValidationMessage;

            using var lotDoc = JsonDocument.Parse(request);
            var jsonLot = lotDoc.RootElement;
            var _lotDetail = AuctioneerUtility.GetPropertyCaseInsensitive(jsonLot, nameof(ProductDetail));
            string lotDetail = !string.IsNullOrEmpty(_lotDetail.Value.ToString()) ? _lotDetail.Value.ToString() : "{}";

            // If constructor throws error related to RuleEngineException and ArgumentNullException.
            ProductContext auctioneerContext = new ProductContext(lotDetail, _requestPipe, _rules, _transformationRules, false);

            // Evaluate LotModel against all Platform rules. If any rule fails exception handled by evaluate method itself
            ruleValidationMessage = await auctioneerContext.EvaluateAsync();

            // Set default values to avoid inputs from user's end for internal properties
            lotModel.LotDetail = auctioneerContext.LotDetail;
            lotModel = AuctioneerUtility.AddDefaultState(_lotDetail.Value, lotModel, _requestPipe.CorrelationId);

            // Create lot
            await _lotDataAccess.CreateAsync(lotModel, cancellationToken);

            EditedLotResponse editedLotResponse = new EditedLotResponse
            {
                ValidationResults = ruleValidationMessage.ValidationResults,
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                LotDetail = lotModel.LotDetail,
                BiddingStates = lotModel.BiddingStates,
                IsValid = ruleValidationMessage.IsValid,
            };

            return editedLotResponse;
        }

        private async Task DeleteAsync(long AuctionId, long LotId, CancellationToken cancellationToken)
        {
            await _lotDataAccess.DeleteAsync(AuctionId, LotId, cancellationToken);
        }

        private async Task DeleteByPartitionKeyAsync(long AuctionId, long LotId, CancellationToken cancellationToken)
        {
            await _lotDataAccess.DeleteByPartitionKeyAsync(AuctionId, LotId, cancellationToken);
        }

        #endregion

        #region Auctioneer Activity
        public async Task<(object, ProductDetail)> UpdateWithRetryAsync(LotRequest request, CancellationToken cancellationToken)
        {
            return await UpdateAsync(request, cancellationToken);
        }

        public async Task<(RuleValidationMessage ruleValidationMessage, EditedLotResponse editedLotResponse)> ValidateAndManipulate(LotRequest request, ProductModel latestLotModel = null, string requestId = null, DateTime? timestamp = null)
        {
            string requestString = request.LotDetail.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail lotDetail) = await ValidateAndTransform(requestString, request.PlatformCode);

            (int _, EditedLotResponse editedLotResponse) = await ManipulateLotAsync(request, lotDetail, latestLotModel, requestId, timestamp);
            return (ruleValidationMessage, editedLotResponse);
        }

        public async Task<(int, EditedLotResponse editedLotResponse)> ManipulateLotAsync(LotRequest request, ProductDetail lotDetail, ProductModel latestLotModel = null, string requestId = null, DateTime? timestamp = null)
        {
            var existingLot = await GetAsync(lotDetail.AuctionId, lotDetail.LotId);
            int previousGoodStateSequenceNumber = existingLot.BiddingStates[^1].SequenceNumber;

            existingLot.LotDetail.Quantity = lotDetail.Quantity;
            existingLot.LotDetail.Increment = lotDetail.Increment;
            existingLot.LotDetail.OpeningPrice = lotDetail.OpeningPrice;
            existingLot.LotDetail.ReservePrice = lotDetail.ReservePrice;

            EditedLotResponse editedLotResponse = new EditedLotResponse();
            editedLotResponse.IsValid = true;
            editedLotResponse.LotDetail = existingLot.LotDetail;
            editedLotResponse.BiddingStates = existingLot.BiddingStates;
            editedLotResponse.SchemaType = SchemaTypes.EditLot;
            editedLotResponse.ETag = existingLot.ETag;

            return (previousGoodStateSequenceNumber, editedLotResponse);
        }

        public async Task<LotResponse> ValidateAndTransformLotDetail(dynamic request)
        {
            // request is the dynamic lot detail object
            string requestString = request.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail lotDetail) = await ValidateAndTransform(requestString);

            return new LotResponse
            {
                IsValid = ruleValidationMessage.IsValid,
                ValidationResults = ruleValidationMessage.ValidationResults,
                LotDetail = lotDetail,
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId
            };
        }

        private async Task<(object, ProductDetail)> UpdateAsync(LotRequest request, CancellationToken cancellationToken)
        {
            string requestString = request.LotDetail is null ? "{}" : request.LotDetail.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail lotDetail) = await ValidateAndTransform(requestString, request.PlatformCode);

            (int _, EditedLotResponse editedLotResponse) = await ManipulateLotAsync(request, lotDetail);
            await _lotDataAccess.UpsertAsync(editedLotResponse, cancellationToken);

            // Create api response 
            editedLotResponse.ValidationResults.AddRange(ruleValidationMessage.ValidationResults);
            editedLotResponse.TimeStamp = DateTime.UtcNow;
            editedLotResponse.RequestId = _requestPipe.CorrelationId;
            return (editedLotResponse, editedLotResponse.LotDetail);
        }

        public async Task<(RuleValidationMessage, ProductDetail)> ValidateAndTransform(string request, string PlatformCode = "0", bool validateAuctionDetail = false)
        {
            // If constructor throws error, it'll be handled by ExceptionMiddleware
            // request is the dynamic lot detail object
            ProductContext auctioneerContext = new ProductContext(request, _requestPipe, _rules, _transformationRules, validateAuctionDetail);
            // Evaluate LotModel against all Platform rules. If any rule fails exception handled by evaluate method itself
            RuleValidationMessage ruleValidationMessage = await auctioneerContext.EvaluateAsync();

            return (ruleValidationMessage, auctioneerContext.LotDetail);
        }

        #endregion
    }
}
