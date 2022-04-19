using System.Threading;
using System.Threading.Tasks;
using Demo.MedTech.DataModel.Shared;

namespace Demo.MedTech.DAL
{
    public interface ILotDataAccess
    {
        Task<LotModel> GetAsync(long AuctionId, long LotId);

        Task CreateAsync(LotModel lotModel, CancellationToken cancellationToken);

        Task DeleteAsync(long AuctionId, long LotId, CancellationToken cancellationToken);

        Task<LotModel> GetByIdAsync(string documentId, long AuctionId, long LotId);

        Task DeleteByPartitionKeyAsync(long AuctionId, long LotId, CancellationToken cancellationToken);

        Task<string> UpsertAsync(LotModel lotModel, CancellationToken cancellationToken, bool isRetraction = false, string documentId = "");
    }
}