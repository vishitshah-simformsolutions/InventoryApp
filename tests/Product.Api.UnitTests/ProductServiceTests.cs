using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Product.DAL;
using Product.DataModel.Exceptions;
using Product.DataModel.Response;
using Product.DataModel.Shared;
using Product.Utility.Helper;
using Product.ValidationEngine.Rules;
using Moq;
using Xunit;
using Product.UnitTests;
using Product.Service;

namespace Product.Api.UnitTests
{
    public class ProductServiceTests : IClassFixture<ProductServiceTests.ProductServiceFixture>
    {
        private static IProductService _productService;
        private readonly Mock<IRequestPipe> _mockRequestPipe;
        private readonly IList<IRule> _rules;
        private readonly IList<ITransform> _transformRules;
        private static IRequestPipe _requestPipe;

        public ProductServiceTests(ProductServiceFixture productServiceFixture)
        {
            _productService = productServiceFixture.ProductService;
            _mockRequestPipe = productServiceFixture.MockRequestPipe;
            _requestPipe = new RequestPipe();
            _rules = typeof(IRule).Assembly.GetTypes()
                .Where(t => typeof(IRule).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as IRule).ToList();
            _transformRules = typeof(ITransform).Assembly.GetTypes()
                .Where(t => typeof(ITransform).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as ITransform).ToList();
        }

        
        #region TestData

        public static IEnumerable<object[]> CreateLotTestData =>
            new List<object[]>
            {
                new object[]
                {
                    CommonUtilities.CreateProductDetail(itemId:11,productId:10,openingPrice:10,buyItNow:null,quantity:1,timeZone:"UTC",extensionTimeInSeconds:600,reservePrice:20,increments:null,startTime:DateTime.UtcNow,endsFrom:DateTime.UtcNow.AddDays(7)),
                }
            };

        [Theory]
        [MemberData(nameof(CreateLotTestData))]
        public void Given_valid_product_is_passed_and_product_already_exists_When_CreateAsync_is_called_Then_product_should_not_be_created_and_product_exists_validation_should_be_in_validation_results
           (ProductDetail productDetail)
        {
            //Arrange
            var lotRequest = CommonUtilities.CreateRequestModel(productDetail);
            const int lotAlreadyExistsCode = 164;
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new DateTimeConverter());
            var requestString = JsonSerializer.Serialize(lotRequest, options);
            var mockLotDataAccess = new Mock<ILotDataAccess>();
            var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(lotAlreadyExistsCode) };
            var dictionarySuggestion = new Dictionary<string, object>
            {
                {"ProductId",productDetail.ProductId},
                {"ItemId",productDetail.ItemId},
            };
            validationResult[0].Data = dictionarySuggestion;
            mockLotDataAccess.Setup(x => x.CreateAsync(It.IsAny<ProductModel>(), It.IsAny<CancellationToken>())).ThrowsAsync(new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, new Exception("Rule engine exception")));
            var productService = CreateProductService(mockLotDataAccess);

            //Act
            var caughtException = Assert.ThrowsAsync<RuleEngineException>(async () => await productService.InsertWithRetryAsync(requestString, new CancellationToken()));
            var ruleValidationMessage = caughtException.Result.RuleValidationMessage.ValidationResults.FirstOrDefault();

            //Assert
            Assert.Equal(lotAlreadyExistsCode, ruleValidationMessage?.Code);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == lotAlreadyExistsCode)?.Value, ruleValidationMessage?.Value);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == lotAlreadyExistsCode)?.Description, ruleValidationMessage?.Description);
            Assert.Equal(dictionarySuggestion, ruleValidationMessage?.Data);
            mockLotDataAccess.Verify(x => x.CreateAsync(It.IsAny<ProductModel>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Given_invalid_product_id_or_item_id_is_passed_When_GetAsync_is_called_Then_lot_not_found_validation_should_be_in_validation_results
            ()
        {
            //Arrange
            long AuctionId = 1;
            long LotId = 1;
            int lotNotFound = 105;
            var mockLotDataAccess = new Mock<ILotDataAccess>();
            var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(lotNotFound) };
            Dictionary<string, object> dictionarySuggestion = new Dictionary<string, object>
            {
                {"ProductId", AuctionId},
                {"ItemId", LotId},
            };
            validationResult[0].Data = dictionarySuggestion;
            mockLotDataAccess.Setup(x => x.GetAsync(It.IsAny<long>(), It.IsAny<long>())).ThrowsAsync(new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, new Exception("Rule engine exception")));
            var productService = CreateProductService(mockLotDataAccess);

            //Act
            var caughtException = Assert.ThrowsAsync<RuleEngineException>(async () => await productService.GetAsync(AuctionId, LotId));
            var ruleValidationMessage = caughtException.Result.RuleValidationMessage.ValidationResults.FirstOrDefault();

            //Assert
            Assert.Equal(lotNotFound, ruleValidationMessage?.Code);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == lotNotFound)?.Value, ruleValidationMessage?.Value);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == lotNotFound)?.Description, ruleValidationMessage?.Description);
            Assert.Equal(dictionarySuggestion, ruleValidationMessage?.Data);
            mockLotDataAccess.Verify(x => x.GetAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
        }

        [Fact]
        public async Task Given_valid_product_id_or_item_id_is_passed_When_GetAsync_is_called_Then_lot_models_should_be_returned
            ()
        {
            //Arrange
            long AuctionId = 1;
            long LotId = 1;
            var mockLotDataAccess = new Mock<ILotDataAccess>();
            mockLotDataAccess.Setup(x => x.GetAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(new EditedProductResponse());
            var productService = CreateProductService(mockLotDataAccess);

            //Act
            var getResponse = await productService.GetAsync(AuctionId, LotId);

            //Assert
            Assert.NotNull(getResponse);
            mockLotDataAccess.Verify(x => x.GetAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
        }

        [Fact]
        public async Task Given_invalid_product_id_or_item_id_is_passed_When_DeleteAsync_is_called_Then_lot_not_found_validation_should_be_in_validation_results
            ()
        {
            //Arrange
            long AuctionId = 1;
            long LotId = 1;
            int lotNotFound = 105;
            var mockLotDataAccess = new Mock<ILotDataAccess>();
            var validationResult = new List<ValidationResult> { Response.PrepareValidationResult(lotNotFound) };
            var dictionarySuggestion = new Dictionary<string, object>
            {
                {"ProductId", AuctionId},
                {"ItemId", LotId},
            };
            validationResult[0].Data = dictionarySuggestion;
            mockLotDataAccess.Setup(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).ThrowsAsync(new RuleEngineException(new RuleValidationMessage { IsValid = false, ValidationResults = validationResult }, new Exception("Rule engine exception")));
            var productService = CreateProductService(mockLotDataAccess);

            //Act
            var caughtException = Assert.ThrowsAsync<RuleEngineException>(async () => await productService.DeleteWithRetryAsync(AuctionId, LotId, new CancellationToken()));
            var ruleValidationMessage = caughtException.Result.RuleValidationMessage.ValidationResults.FirstOrDefault();

            //Assert
            Assert.Equal(lotNotFound, ruleValidationMessage?.Code);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == lotNotFound)?.Value, ruleValidationMessage?.Value);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == lotNotFound)?.Description, ruleValidationMessage?.Description);
            Assert.Equal(dictionarySuggestion, ruleValidationMessage?.Data);
            mockLotDataAccess.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Given_valid_product_id_or_item_id_is_passed_When_DeleteAsync_is_called_Then_lot_models_should_be_returned
            ()
        {
            //Arrange
            long AuctionId = 1;
            long LotId = 1;
            var mockLotDataAccess = new Mock<ILotDataAccess>();
            mockLotDataAccess.Setup(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()));
            var productService = CreateProductService(mockLotDataAccess);

            //Act
            await productService.DeleteWithRetryAsync(AuctionId, LotId, new CancellationToken());

            //Assert
            mockLotDataAccess.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        public class ProductServiceFixture
        {
            internal Mock<IRequestPipe> MockRequestPipe;
            internal IProductService ProductService;
            internal IList<IRule> _rules;
            internal IList<ITransform> _transformRules;

            private readonly int RetryCount = 5;
            private readonly int TimeoutPeriodInSeconds = 5;
            public ProductServiceFixture()
            {
                //Initialise all properties of 
                MockRequestPipe = new Mock<IRequestPipe>();
                var mockLotDataAccess = new Mock<ILotDataAccess>();
                mockLotDataAccess.Setup(x => x.GetAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(CommonUtilities.CreateLot());
                _rules = typeof(IRule).Assembly.GetTypes()
                    .Where(t => typeof(IRule).IsAssignableFrom(t) && t.IsClass)
                    .Select(t => Activator.CreateInstance(t) as IRule).ToList();
                _transformRules = typeof(ITransform).Assembly.GetTypes()
                    .Where(t => typeof(ITransform).IsAssignableFrom(t) && t.IsClass)
                    .Select(t => Activator.CreateInstance(t) as ITransform).ToList();
                ProductService = new ProductService(mockLotDataAccess.Object, MockRequestPipe.Object, _rules,_transformRules);
            }
        }

        private IProductService CreateProductService(Mock<ILotDataAccess> mockLotDataAccess)
        {
            return new ProductService(mockLotDataAccess.Object, _mockRequestPipe.Object, _rules, _transformRules);
        }
    }
}