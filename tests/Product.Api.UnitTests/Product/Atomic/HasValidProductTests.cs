using System;
using System.Collections.Generic;
using System.Linq;
using Product.UnitTests;
using Product.Utility.Helper;
using Product.ValidationEngine.Model;
using Product.ValidationEngine.Rules;
using Product.ValidationEngine.Rules.Product.Atomic;
using Xunit;

namespace Product.Api.UnitTests.Product.Atomic
{
    public class HasValidProductTests
    {
        private static readonly int HasValidProductCode = 103;
        private readonly IList<IRule> _rules;
        private readonly IList<ITransform> _transformRules;
        private static IRequestPipe _requestPipe;

        public HasValidProductTests()
        {
            _requestPipe = new RequestPipe();
            _rules = typeof(IRule).Assembly.GetTypes()
                .Where(t => typeof(IRule).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as IRule).ToList();
            _transformRules = typeof(ITransform).Assembly.GetTypes()
                .Where(t => typeof(ITransform).IsAssignableFrom(t) && t.IsClass)
                .Select(t => Activator.CreateInstance(t) as ITransform).ToList();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-2147483647)] //int.MaxValue
        public void Given_invalid_product_id_When_mandatory_data_passed_Then_should_return_validation_error(int productId)
        {
            //Arrange
            var productContext = new ProductContext(CommonUtilities.CreateProductDetailString
                (productId: productId, itemId: 10, sellingPrice: 35, buyItNow: 100, quantity: 1, timeZone: "UTC", extensionTimeInSeconds: 600, AuctionHouseId: Guid.NewGuid()), _requestPipe, _rules, _transformRules);
            //Act
            var hasValidProductId = new HasValidProduct();
            var result = hasValidProductId.Execute(productContext);

            //Assert
            Assert.False(result.IsValid);
            Assert.Equal(HasValidProductCode, result.ValidationResults.FirstOrDefault()?.Code);
            Assert.Equal(result.ValidationResults.FirstOrDefault(x => x.Code == HasValidProductCode)?.Value, result.ValidationResults.FirstOrDefault()?.Value);
            Assert.Equal(result.ValidationResults.FirstOrDefault(x => x.Code == HasValidProductCode)?.Description, result.ValidationResults.FirstOrDefault()?.Description);
        }
    }
}
