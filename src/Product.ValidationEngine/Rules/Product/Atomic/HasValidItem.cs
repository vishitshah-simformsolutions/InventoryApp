using System.Linq;
using Product.DataModel.Shared;
using Product.Utility.Helper;
using Product.ValidationEngine.Model;

namespace Product.ValidationEngine.Rules.Product.Atomic
{
    public class HasValidItem : IRule
    {
        private const int HasValidLotErrorCode = 104;
        
        public RuleValidationMessage Execute(ProductContext productContext)
        {
            RuleValidationMessage ruleValidationMessage = new RuleValidationMessage() { IsValid = true };

            if (!(productContext?.ProductDetail?.ItemId <= 0))
            {
                return ruleValidationMessage;
            }

            ruleValidationMessage.IsValid = false;
            ruleValidationMessage.ValidationResults.AddRange(Response.ValidationResults.Where(x => x.Code == HasValidLotErrorCode));

            return ruleValidationMessage;
        }
    }
}