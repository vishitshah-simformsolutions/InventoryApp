using Product.DataModel.Shared;

namespace Playground.Models
{
    public class ProductResponseModel: ProductModel
    {
        public string Domain { get; set; }
        public string SubDomain { get; set; }
        public long ProductId { get; set; }
        public long LotId { get; set; }
    }
}
