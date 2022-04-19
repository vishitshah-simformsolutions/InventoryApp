using Demo.MedTech.DataModel.Request;
using Demo.MedTech.DataModel.Response;
using Demo.MedTech.DataModel.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.MedTech.Service
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
