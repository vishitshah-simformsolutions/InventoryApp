using Product.DataModel.Shared;
using Product.Utility.Helper;
using Product.ValidationEngine.Model;
using System.Linq;

namespace Product.ValidationEngine.Rules.Product.Atomic
{
    public class HasValidQuantity : IRule
    {
        private const int HasValidQuantityErrorCode = 158;

        public RuleValidationMessage Execute(ProductContext productContext)
        {
            RuleValidationMessage ruleValidationMessage = new RuleValidationMessage() { IsValid = true };

            if (productContext.ProductDetail.Quantity < 0)
            {
                ruleValidationMessage.IsValid = false;
                ruleValidationMessage.ValidationResults.AddRange(
                    Response.ValidationResults.Where(x => x.Code == HasValidQuantityErrorCode));
            }

            return ruleValidationMessage;
        }
    }
}