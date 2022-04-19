using System.Linq;
using Product.DataModel.Shared;
using Product.Utility.Helper;
using Product.ValidationEngine.Model;

namespace Product.ValidationEngine.Rules.Auctioneer.Atomic
{
    public class HasValidAuction : IRule
    {
        private const int HasValidAuctionErrorCode = 103;

        public RuleValidationMessage Execute(ProductContext auctioneerContext)
        {
            RuleValidationMessage ruleValidationMessage = new RuleValidationMessage() { IsValid = true };
            
            if (!(auctioneerContext?.LotDetail?.AuctionId <= 0))
            {
                return ruleValidationMessage;
            }

            ruleValidationMessage.IsValid = false;
            ruleValidationMessage.ValidationResults.AddRange(Response.ValidationResults.Where(x => x.Code == HasValidAuctionErrorCode));

            return ruleValidationMessage;
        }
    }
}