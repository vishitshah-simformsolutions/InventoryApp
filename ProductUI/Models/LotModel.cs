using Product.DataModel.Shared;

namespace Playground.Models
{
    public class EgressLotModel: ProductModel
    {
        public string Domain { get; set; }
        public string SubDomain { get; set; }
        public long AuctionId { get; set; }
        public long LotId { get; set; }
    }
}
