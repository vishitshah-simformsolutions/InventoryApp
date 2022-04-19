using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.ValidationEngine.Model;

namespace Demo.MedTech.ValidationEngine.Rules
{
    public interface IRule
    {
        RuleValidationMessage Execute(ProductContext auctioneerContext);
    }
}