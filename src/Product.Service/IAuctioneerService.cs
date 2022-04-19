using Product.DataModel.Request;
using Product.DataModel.Response;
using Product.DataModel.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Service
{
    public interface IAuctioneerService
    {
        Task<EditedLotResponse> InsertWithRetryAsync(string request, CancellationToken cancellationToken);

        Task<EditedLotResponse> GetAsync(long AuctionId, long LotId);

        Task<EditedLotResponse> GetByIdAsync(string documentId, long AuctionId, long LotId);

        Task DeleteWithRetryAsync(long AuctionId, long LotId, CancellationToken cancellationToken);


        Task DeleteByPartitionKeyWithRetryAsync(long AuctionId, long LotId, CancellationToken cancellationToken);

        Task<(object,LotDetail)> UpdateWithRetryAsync(LotRequest request, CancellationToken cancellationToken);

        Task<LotResponse> ValidateAndTransformLotDetail(dynamic request);
    }
}
