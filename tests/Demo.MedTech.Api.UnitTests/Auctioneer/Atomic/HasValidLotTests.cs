using System;
using System.Collections.Generic;
using System.Linq;
using Product.UnitTests;
using Product.Utility.Helper;
using Product.ValidationEngine.Model;
using Product.ValidationEngine.Rules;
using Product.ValidationEngine.Rules.Auctioneer.Atomic;
using Xunit;

namespace Product.Api.UnitTests.Auctioneer.Atomic
{
    public class HasValidLotTests
    {
        private static readonly int HasValidAuctionHouseIdErrorCode = 104;
        private readonly IList<IRule> _rules;
        private readonly IList<ITransform> _transformRules;
        private static IRequestPipe _requestPipe;

        public HasValidLotTests()
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
        public void Given_invalid_lot_id_When_mandatory_data_passed_Then_should_return_validation_error(int LotId)
        {
            //Arrange
            var auctioneerContext = new ProductContext(CommonUtilities.CreateLotDetailString
                (AuctionId: 10, LotId: LotId, openingPrice: 35, buyItNow: 100, quantity: 1, timeZone: "UTC", extensionTimeInSeconds: 600, AuctionHouseId: Guid.NewGuid()), _requestPipe, _rules, _transformRules);
            //Act
            var hasValidLotId = new HasValidLot();
            var result = hasValidLotId.Execute(auctioneerContext);

            //Assert
            Assert.False(result.IsValid);
            Assert.Equal(HasValidAuctionHouseIdErrorCode, result.ValidationResults.FirstOrDefault()?.Code);
            Assert.Equal(result.ValidationResults.FirstOrDefault(x => x.Code == HasValidAuctionHouseIdErrorCode)?.Value, result.ValidationResults.FirstOrDefault()?.Value);
            Assert.Equal(result.ValidationResults.FirstOrDefault(x => x.Code == HasValidAuctionHouseIdErrorCode)?.Description, result.ValidationResults.FirstOrDefault()?.Description);
        }
    }
}
