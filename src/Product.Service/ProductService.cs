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
        private readonly ILotDataAccess _productDataAccess;
        private readonly IRequestPipe _requestPipe;

        public ProductService
            (
                ILotDataAccess productDataAccess,
                IRequestPipe requestPipe,
                IEnumerable<IRule> rules,
                IEnumerable<ITransform> transformationRules
            )
        {
            _productDataAccess = productDataAccess;
            _requestPipe = requestPipe;
            _rules = rules;
            _transformationRules = transformationRules;
        }

        #region Ingress

        public async Task<EditedProductResponse> InsertWithRetryAsync(string request, CancellationToken cancellationToken)
        {
            return await CreateAsync(request, cancellationToken);
        }

        public async Task DeleteWithRetryAsync(long productId, long itemId, CancellationToken cancellationToken)
        {
            await DeleteAsync(productId, itemId, cancellationToken);
        }

        public async Task DeleteByPartitionKeyWithRetryAsync(long productId, long itemId, CancellationToken cancellationToken)
        {
            await DeleteByPartitionKeyAsync(productId, itemId, cancellationToken);
        }

        public async Task<EditedProductResponse> GetAsync(long productId, long itemId)
        {
            var productModel = await _productDataAccess.GetAsync(productId, itemId);
            return new EditedProductResponse
            {
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                ProductDetail = productModel.ProductDetail
            };
        }

        public async Task<EditedProductResponse> GetByIdAsync(string documentId, long productId, long itemId)
        {
            var productModel = await _productDataAccess.GetByIdAsync(documentId, productId, itemId);
            return new EditedProductResponse
            {
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                ProductDetail = productModel.ProductDetail
            };
        }



        private async Task<EditedProductResponse> CreateAsync(string request, CancellationToken cancellationToken)
        {
            ProductModel productModel = new ProductModel();
            RuleValidationMessage ruleValidationMessage;

            using var lotDoc = JsonDocument.Parse(request);
            var jsonLot = lotDoc.RootElement;
            var _productDetail = ProductUtility.GetPropertyCaseInsensitive(jsonLot, nameof(ProductDetail));
            string productDetail = !string.IsNullOrEmpty(_productDetail.Value.ToString()) ? _productDetail.Value.ToString() : "{}";

            // If constructor throws error related to RuleEngineException and ArgumentNullException.
            ProductContext productContext = new ProductContext(productDetail, _requestPipe, _rules, _transformationRules, false);

            // Evaluate productModel against all Platform rules. If any rule fails exception handled by evaluate method itself
            ruleValidationMessage = await productContext.EvaluateAsync();

            // Set default values to avoid inputs from user's end for internal properties
            productModel.ProductDetail = productContext.ProductDetail;
            productModel = ProductUtility.AddDefaultState(_productDetail.Value, productModel, _requestPipe.CorrelationId);

            // Create lot
            await _productDataAccess.CreateAsync(productModel, cancellationToken);

            EditedProductResponse editedProductResponse = new EditedProductResponse
            {
                ValidationResults = ruleValidationMessage.ValidationResults,
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId,
                ProductDetail = productModel.ProductDetail,
                IsValid = ruleValidationMessage.IsValid,
            };

            return editedProductResponse;
        }

        private async Task DeleteAsync(long productId, long itemId, CancellationToken cancellationToken)
        {
            await _productDataAccess.DeleteAsync(productId, itemId, cancellationToken);
        }

        private async Task DeleteByPartitionKeyAsync(long productId, long itemId, CancellationToken cancellationToken)
        {
            await _productDataAccess.DeleteByPartitionKeyAsync(productId, itemId, cancellationToken);
        }

        #endregion

        #region Product Activity
        public async Task<(object, ProductDetail)> UpdateWithRetryAsync(ProductRequest request, CancellationToken cancellationToken)
        {
            return await UpdateAsync(request, cancellationToken);
        }

        public async Task<(RuleValidationMessage ruleValidationMessage, EditedProductResponse editedLotResponse)> ValidateAndManipulate(ProductRequest request, ProductModel latestproductModel = null, string requestId = null, DateTime? timestamp = null)
        {
            string requestString = request.ProductDetail.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail productDetail) = await ValidateAndTransform(requestString, "0");

            (int _, EditedProductResponse editedLotResponse) = await ManipulateLotAsync(request, productDetail, latestproductModel, requestId, timestamp);
            return (ruleValidationMessage, editedLotResponse);
        }

        public async Task<(int, EditedProductResponse editedLotResponse)> ManipulateLotAsync(ProductRequest request, ProductDetail productDetail, ProductModel latestproductModel = null, string requestId = null, DateTime? timestamp = null)
        {
            var existingLot = await GetAsync(productDetail.ProductId, productDetail.ItemId);

            existingLot.ProductDetail.Quantity = productDetail.Quantity;
            existingLot.ProductDetail.Increment = productDetail.Increment;
            existingLot.ProductDetail.SellingPrice = productDetail.SellingPrice;
            existingLot.ProductDetail.ManufacturingPrice = productDetail.ManufacturingPrice;

            EditedProductResponse editedProductResponse = new EditedProductResponse();
            editedProductResponse.IsValid = true;
            editedProductResponse.ProductDetail = existingLot.ProductDetail;
            editedProductResponse.ETag = existingLot.ETag;

            return (0, editedProductResponse);
        }

        public async Task<ProductResponse> ValidateProductDetail(dynamic request)
        {
            // request is the dynamic lot detail object
            string requestString = request.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail productDetail) = await ValidateAndTransform(requestString);

            return new ProductResponse
            {
                IsValid = ruleValidationMessage.IsValid,
                ValidationResults = ruleValidationMessage.ValidationResults,
                ProductDetail = productDetail,
                TimeStamp = DateTime.UtcNow,
                RequestId = _requestPipe.CorrelationId
            };
        }

        private async Task<(object, ProductDetail)> UpdateAsync(ProductRequest request, CancellationToken cancellationToken)
        {
            string requestString = request.ProductDetail is null ? "{}" : request.ProductDetail.ToString();
            (RuleValidationMessage ruleValidationMessage, ProductDetail productDetail) = await ValidateAndTransform(requestString, "0");

            (int _, EditedProductResponse editedLotResponse) = await ManipulateLotAsync(request, productDetail);
            await _productDataAccess.UpsertAsync(editedLotResponse, cancellationToken);

            // Create api response 
            editedLotResponse.ValidationResults.AddRange(ruleValidationMessage.ValidationResults);
            editedLotResponse.TimeStamp = DateTime.UtcNow;
            editedLotResponse.RequestId = _requestPipe.CorrelationId;
            return (editedLotResponse, editedLotResponse.ProductDetail);
        }

        public async Task<(RuleValidationMessage, ProductDetail)> ValidateAndTransform(string request, string PlatformCode = "0", bool validateProductDetail = false)
        {
            // If constructor throws error, it'll be handled by ExceptionMiddleware
            // request is the dynamic lot detail object
            ProductContext productContext = new ProductContext(request, _requestPipe, _rules, _transformationRules, validateProductDetail);
            // Evaluate productModel against all Platform rules. If any rule fails exception handled by evaluate method itself
            RuleValidationMessage ruleValidationMessage = await productContext.EvaluateAsync();

            return (ruleValidationMessage, productContext.ProductDetail);
        }

        #endregion
    }
}
