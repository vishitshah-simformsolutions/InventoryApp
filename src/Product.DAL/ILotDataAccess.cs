using System.Threading;
using System.Threading.Tasks;
using Product.DataModel.Shared;

namespace Product.DAL
{
    public interface ILotDataAccess
    {
        Task<ProductModel> GetAsync(long AuctionId, long LotId);

        Task CreateAsync(ProductModel lotModel, CancellationToken cancellationToken);

        Task DeleteAsync(long AuctionId, long LotId, CancellationToken cancellationToken);

        Task<ProductModel> GetByIdAsync(string documentId, long AuctionId, long LotId);

        Task DeleteByPartitionKeyAsync(long AuctionId, long LotId, CancellationToken cancellationToken);

        Task<string> UpsertAsync(ProductModel lotModel, CancellationToken cancellationToken, bool isRetraction = false, string documentId = "");
    }
}