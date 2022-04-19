using Product.DataModel.Request;
using Product.DataModel.Response;
using Product.DataModel.Shared;
using Product.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v1")]
    [ExcludeFromCodeCoverage]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Route("product")]
        [ProducesResponseType(typeof(EditedProductResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> Post(dynamic request, CancellationToken cancellationToken)
        {
            EditedProductResponse productResponse = await _productService.InsertWithRetryAsync(request.ToString(), cancellationToken);


            return Ok(productResponse);
        }

        [HttpGet]
        [Route("product")]
        [ProducesResponseType(typeof(EditedProductResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> Get(long auctionId, long lotId)
        {
            var lotResponse = await _productService.GetAsync(auctionId, lotId);
            return Ok(lotResponse);
        }

        [HttpGet]
        [Route("productbyid")]
        [ProducesResponseType(typeof(EditedProductResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> Get(string documentId, long auctionId, long lotId)
        {
            var lotResponse = await _productService.GetByIdAsync(documentId, auctionId, lotId);
            return Ok(lotResponse);
        }

        [HttpDelete]
        [Route("product")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete(long auctionId, long lotId, CancellationToken cancellationToken)
        {
            await _productService.DeleteWithRetryAsync(auctionId, lotId, cancellationToken);

            return Ok();
        }

        [HttpDelete]
        [Route("productbypartitionkey")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteByPartitionKey(long auctionId, long lotId, CancellationToken cancellationToken)
        {
            await _productService.DeleteByPartitionKeyWithRetryAsync(auctionId, lotId, cancellationToken);

           

            return Ok();
        }

        
        /// <summary>
        /// Validate and transform ProductDetail API
        /// </summary>
        /// <param name="request">Validate ProductDetail request</param>
        /// <returns>Return lot response if ProductDetail is valid</returns>
        [HttpPost]
        [Route("validateproductdetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ValidateAndTransformLotDetail([FromBody] dynamic request)
        {
            ProductResponse productResponse = await _productService.ValidateAndTransformLotDetail(request);
            return Ok(productResponse);
        }

        /// <summary>
        /// Edit Lot API
        /// </summary>
        /// <param name="request">When auctioneer places a request to edit lot</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Return ProductResponse if lot is valid</returns>
        [HttpPost]
        [Route("updateproduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateLot([FromBody] ProductRequest request, CancellationToken cancellationToken)
        {
            (object editedLotResponse,ProductDetail lotDetail) = await _productService.UpdateWithRetryAsync(request, cancellationToken);

         

            return Ok(editedLotResponse);
        }
    }
}