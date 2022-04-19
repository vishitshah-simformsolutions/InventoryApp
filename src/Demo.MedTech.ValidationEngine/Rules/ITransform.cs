using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.ValidationEngine.Model;

namespace Demo.MedTech.ValidationEngine.Rules
{
    public interface ITransform
    {
        RuleValidationMessage Transform(ProductContext auctioneerContext);
    }
}