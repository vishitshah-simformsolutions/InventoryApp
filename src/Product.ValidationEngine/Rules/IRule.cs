using Product.DataModel.Shared;
using Product.ValidationEngine.Model;

namespace Product.ValidationEngine.Rules
{
    public interface IRule
    {
        RuleValidationMessage Execute(ProductContext auctioneerContext);
    }
}