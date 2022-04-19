using System;
using System.Collections.Generic;
using System.Linq;
using Demo.MedTech.Utility.Helper;
using Demo.MedTech.ValidationEngine.Model;
using Demo.MedTech.ValidationEngine.Rules;
using Demo.MedTech.ValidationEngine.Rules.Auctioneer.Atomic;
using Xunit;

namespace Demo.MedTech.Api.UnitTests.Auctioneer.Atomic
{
    public class HasValidQuantityTests
    {
        private static readonly int HasValidQuantityErrorCode = 158;
        private readonly IList<IRule> _rules;
        private readonly IList<ITransform> _transformRules;
        private static IRequestPipe _requestPipe;

        public HasValidQuantityTests()
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
        [InlineData(1, 1, 20, 200, 0, "IST", 0)]
        [InlineData(1, 1, 20, 200, 5, "IST", 0)]
        [InlineData(1, 1, 20, 200, 5.5, "IST", 0)]
        [InlineData(1, 1, 20, 200, 100, "IST", 0)]
        public void Given_valid_quantity_is_passed_When_has_valid_quantity_executed_Then_should_not_return_validation_error(long AuctionId, long LotId, decimal openingPrice, decimal buyItNow, decimal quantity, string timeZone, int extensionTimeInSeconds)
        {
            //Arrange
            var auctioneerContext = new ProductContext(CommonUtilities.CreateLotDetailString(AuctionId, LotId, openingPrice, buyItNow, quantity, timeZone, extensionTimeInSeconds),_requestPipe, _rules, _transformRules);

            //Act
            var hasValidQuantity = new HasValidQuantity();
            var result = hasValidQuantity.Execute(auctioneerContext);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(1, 1, 20, 200, null, "IST", 0)]
        public void Given_null_is_passed_in_quantity_When_has_valid_quantity_executed_Then_should_not_return_validation_error(long AuctionId, long LotId, decimal openingPrice, decimal buyItNow, decimal? quantity, string timeZone, int extensionTimeInSeconds)
        {
            //Arrange
            var auctioneerContext = new ProductContext(CommonUtilities.CreateLotDetailString(AuctionId, LotId, openingPrice, buyItNow, quantity, timeZone, extensionTimeInSeconds), _requestPipe, _rules, _transformRules);

            //Act
            var hasValidQuantity = new HasValidQuantity();
            var result = hasValidQuantity.Execute(auctioneerContext);

            //Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(1, 1, 20, 200, -5, "IST", 0)]
        public void Given_invalid_quantity_is_passed_When_has_valid_quantity_executed_Then_should_return_validation_error(long AuctionId, long LotId, decimal openingPrice, decimal buyItNow, decimal quantity, string timeZone, int extensionTimeInSeconds)
        {
            //Arrange
            var auctioneerContext = new ProductContext(CommonUtilities.CreateLotDetailString(AuctionId, LotId, openingPrice, buyItNow, quantity, timeZone, extensionTimeInSeconds), _requestPipe, _rules, _transformRules);

            //Act
            var hasValidQuantity = new HasValidQuantity();
            var result = hasValidQuantity.Execute(auctioneerContext);

            //Assert
            Assert.False(result.IsValid);
            Assert.Equal(HasValidQuantityErrorCode, result.ValidationResults.FirstOrDefault()?.Code);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == HasValidQuantityErrorCode)?.Value, result.ValidationResults.FirstOrDefault()?.Value);
            Assert.Equal(Response.ValidationResults.FirstOrDefault(x => x.Code == HasValidQuantityErrorCode)?.Description, result.ValidationResults.FirstOrDefault()?.Description);
        }
    }
}