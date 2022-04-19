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
    public class ProductService : IProductService
    {
        private readonly IEnumerable<IRule> _rules;
        private readonly IEnumerable<ITransform> _transformationRules;
        private readonly ILotDataAccess _lotDataAccess;
        private readonly IRequestPipe _requestPipe;

        public ProductService
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

        public async Task<EditedProductResponse> InsertWithRetryAsync(string request, CancellationToken cancellationToken)
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

        public async Task<EditedProductResponse> GetAsync(long AuctionId, long LotId)
        {
            var lotModel = await _lotDataAccess.GetAsync(AuctionId, LotId);
            return new EditedProductResponse
            {
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                ProductDetail = lotModel.ProductDetail
            };
        }

        public async Task<EditedProductResponse> GetByIdAsync(string documentId, long AuctionId, long LotId)
        {
            var lotModel = await _lotDataAccess.GetByIdAsync(documentId, AuctionId, LotId);
            return new EditedProductResponse
            {
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                ProductDetail = lotModel.ProductDetail
            };
        }



        private async Task<EditedProductResponse> CreateAsync(string request, CancellationToken cancellationToken)
        {
            ProductModel lotModel = new ProductModel();
            RuleValidationMessage ruleValidationMessage;

            using var lotDoc = JsonDocument.Parse(request);
            var jsonLot = lotDoc.RootElement;
            var _lotDetail = ProductUtility.GetPropertyCaseInsensitive(jsonLot, nameof(ProductDetail));
            string lotDetail = !string.IsNullOrEmpty(_lotDetail.Value.ToString()) ? _lotDetail.Value.ToString() : "{}";

            // If constructor throws error related to RuleEngineException and ArgumentNullException.
            ProductContext productContext = new ProductContext(lotDetail, _requestPipe, _rules, _transformationRules, false);

            // Evaluate LotModel against all Platform rules. If any rule fails exception handled by evaluate method itself
            ruleValidationMessage = await productContext.EvaluateAsync();

            // Set default values to avoid inputs from user's end for internal properties
            lotModel.ProductDetail = productContext.LotDetail;
            lotModel = ProductUtility.AddDefaultState(_lotDetail.Value, lotModel, _requestPipe.CorrelationId);

            // Create lot
            await _lotDataAccess.CreateAsync(lotModel, cancellationToken);

            EditedProductResponse editedProductResponse = new EditedProductResponse
            {
                ValidationResults = ruleValidationMessage.ValidationResults,
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                ProductDetail = lotModel.ProductDetail,
                IsValid = ruleValidationMessage.IsValid,
            };

            return editedProductResponse;
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
        public async Task<(object, ProductDetail)> UpdateWithRetryAsync(ProductRequest request, CancellationToken cancellationToken)
        {
            return await UpdateAsync(request, cancellationToken);
        }

        public async Task<(RuleValidationMessage ruleValidationMessage, EditedProductResponse editedLotResponse)> ValidateAndManipulate(ProductRequest request, ProductModel latestLotModel = null, string requestId = null, DateTime? timestamp = null)
        {
            string requestString = request.ProductDetail.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail lotDetail) = await ValidateAndTransform(requestString, "0");

            (int _, EditedProductResponse editedLotResponse) = await ManipulateLotAsync(request, lotDetail, latestLotModel, requestId, timestamp);
            return (ruleValidationMessage, editedLotResponse);
        }

        public async Task<(int, EditedProductResponse editedLotResponse)> ManipulateLotAsync(ProductRequest request, ProductDetail lotDetail, ProductModel latestLotModel = null, string requestId = null, DateTime? timestamp = null)
        {
            var existingLot = await GetAsync(lotDetail.ProductId, lotDetail.ItemId);

            existingLot.ProductDetail.Quantity = lotDetail.Quantity;
            existingLot.ProductDetail.Increment = lotDetail.Increment;
            existingLot.ProductDetail.SellingPrice = lotDetail.SellingPrice;
            existingLot.ProductDetail.ManufacturingPrice = lotDetail.ManufacturingPrice;

            EditedProductResponse editedProductResponse = new EditedProductResponse();
            editedProductResponse.IsValid = true;
            editedProductResponse.ProductDetail = existingLot.ProductDetail;
            editedProductResponse.ETag = existingLot.ETag;

            return (0, editedProductResponse);
        }

        public async Task<ProductResponse> ValidateAndTransformLotDetail(dynamic request)
        {
            // request is the dynamic lot detail object
            string requestString = request.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail lotDetail) = await ValidateAndTransform(requestString);

            return new ProductResponse
            {
                IsValid = ruleValidationMessage.IsValid,
                ValidationResults = ruleValidationMessage.ValidationResults,
                ProductDetail = lotDetail,
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId
            };
        }

        private async Task<(object, ProductDetail)> UpdateAsync(ProductRequest request, CancellationToken cancellationToken)
        {
            string requestString = request.ProductDetail is null ? "{}" : request.ProductDetail.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail lotDetail) = await ValidateAndTransform(requestString, "0");

            (int _, EditedProductResponse editedLotResponse) = await ManipulateLotAsync(request, lotDetail);
            await _lotDataAccess.UpsertAsync(editedLotResponse, cancellationToken);

            // Create api response 
            editedLotResponse.ValidationResults.AddRange(ruleValidationMessage.ValidationResults);
            editedLotResponse.TimeStamp = DateTime.UtcNow;
            editedLotResponse.RequestId = _requestPipe.CorrelationId;
            return (editedLotResponse, editedLotResponse.ProductDetail);
        }

        public async Task<(RuleValidationMessage, ProductDetail)> ValidateAndTransform(string request, string PlatformCode = "0", bool validateAuctionDetail = false)
        {
            // If constructor throws error, it'll be handled by ExceptionMiddleware
            // request is the dynamic lot detail object
            ProductContext productContext = new ProductContext(request, _requestPipe, _rules, _transformationRules, validateAuctionDetail);
            // Evaluate LotModel against all Platform rules. If any rule fails exception handled by evaluate method itself
            RuleValidationMessage ruleValidationMessage = await productContext.EvaluateAsync();

            return (ruleValidationMessage, productContext.LotDetail);
        }

        #endregion
    }
}
