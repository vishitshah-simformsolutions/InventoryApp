using System.Linq;
using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.Utility.Helper;
using Demo.MedTech.ValidationEngine.Model;

namespace Demo.MedTech.ValidationEngine.Rules.Auctioneer.Atomic
{
    public class HasValidLot : IRule
    {
        private const int HasValidLotErrorCode = 104;
        
        public RuleValidationMessage Execute(ProductContext auctioneerContext)
        {
            RuleValidationMessage ruleValidationMessage = new RuleValidationMessage() { IsValid = true };

            if (!(auctioneerContext?.LotDetail?.LotId <= 0))
            {
                return ruleValidationMessage;
            }

            ruleValidationMessage.IsValid = false;
            ruleValidationMessage.ValidationResults.AddRange(Response.ValidationResults.Where(x => x.Code == HasValidLotErrorCode));

            return ruleValidationMessage;
        }
    }
}