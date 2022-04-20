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

        Task<EditedProductResponse> GetAsync(long productId, long itemId);

        Task<EditedProductResponse> GetByIdAsync(string documentId, long productId, long itemId);

        Task DeleteWithRetryAsync(long productId, long itemId, CancellationToken cancellationToken);


        Task DeleteByPartitionKeyWithRetryAsync(long productId, long itemId, CancellationToken cancellationToken);

        Task<(object,ProductDetail)> UpdateWithRetryAsync(ProductRequest request, CancellationToken cancellationToken);

        Task<ProductResponse> ValidateProductDetail(dynamic request);
    }
}
