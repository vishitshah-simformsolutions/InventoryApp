using System;
using System.Collections.Generic;
using System.Linq;
using Product.DataModel.Exceptions;
using Product.Utility.Helper;
using Product.ValidationEngine.Model;
using Product.ValidationEngine.Rules;
using Xunit;

namespace Product.Api.UnitTests.Product
{
    public class ValidatorTests
    {
        private static readonly int IsValidDataTypeStatusCode = 101;
        private readonly IList<IRule> _rules;
        private readonly IList<ITransform> _transformRules;
        private static IRequestPipe _requestPipe;

        public ValidatorTests()
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
        [InlineData("{\"ProductId\":1,\"ItemId\":30,\"sellingPrice\":20,\"manufacturingPrice\":null,\"increment\":[{\"Low\":0,\"High\":50,\"IncrementValue\":5},{\"Low\":50,\"IncrementValue\":100}],\"quantity\":5}")]
        public void Given_request_has_valid_data_type_When_valid_mandatory_data_is_executed_Then_should_not_return_validation_error(string request)
        {
            //Arrange
            var productContext = new ProductContext(request, _requestPipe, _rules, _transformRules);

            //Assert
            Assert.NotNull(productContext.ProductDetail);
        }

        [Fact]
        public void Given_user_not_provided_input_When_create_object_for_market_place_bidding_context_Then_should_return_null_reference_exception()
        {
            var caughtException = Assert.Throws<ArgumentNullException>(() => new ProductContext(null, _requestPipe, _rules, _transformRules));

            Assert.Equal("Value cannot be null. (Parameter 'lotDetailJson')", caughtException.Message);
        }

        [Theory]
        [InlineData("{\"ProductId\":1,\"ItemId\":null,\"sellingPrice\":20,\"manufacturingPrice\":null,\"increment\":[{\"Low\":0,\"High\":50,\"IncrementValue\":5},{\"Low\":50,\"IncrementValue\":100}],\"quantity\":5}")]
        public void Given_request_has_invalid_data_type_When_valid_mandatory_data_is_executed_Then_should_return_validation_error(string request)
        {
            //Arrange
            var caughtException = Assert.Throws<RuleEngineException>(() =>
                new ProductContext(request, _requestPipe, _rules, _transformRules));

            //Assert
            Assert.False(caughtException.RuleValidationMessage.IsValid);
            Assert.Equal(IsValidDataTypeStatusCode, caughtException.RuleValidationMessage.ValidationResults.FirstOrDefault()?.Code);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == IsValidDataTypeStatusCode)?.Value, caughtException.RuleValidationMessage.ValidationResults.FirstOrDefault()?.Value);
        }
    }
}