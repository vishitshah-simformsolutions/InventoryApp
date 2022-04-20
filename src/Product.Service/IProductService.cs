using Product.DataModel.Request;
using Product.DataModel.Response;
using Product.DataModel.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Service
{
    public interface IProductService
    {
        Task<EditedProductResponse> InsertWithRetryAsync(string request, CancellationToken cancellationToken);

        Task<EditedProductResponse> GetAsync(long AuctionId, long LotId);

        Task<EditedProductResponse> GetByIdAsync(string documentId, long AuctionId, long LotId);

        Task DeleteWithRetryAsync(long AuctionId, long LotId, CancellationToken cancellationToken);


        Task DeleteByPartitionKeyWithRetryAsync(long AuctionId, long LotId, CancellationToken cancellationToken);

        Task<(object,ProductDetail)> UpdateWithRetryAsync(ProductRequest request, CancellationToken cancellationToken);

        Task<ProductResponse> ValidateProductDetail(dynamic request);
    }
}
