using Product.DataModel.Shared;
using Product.ValidationEngine.Model;

namespace Product.ValidationEngine.Rules
{
    public interface ITransform
    {
        RuleValidationMessage Transform(ProductContext productContext);
    }
}