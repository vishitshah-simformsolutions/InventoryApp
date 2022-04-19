using Demo.MedTech.DataModel.Request;
using Demo.MedTech.DataModel.Response;
using Demo.MedTech.DataModel.Shared;
using Demo.MedTech.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.MedTech.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v1")]
    [ExcludeFromCodeCoverage]
    public class AuctioneerController : ControllerBase
    {
        private readonly IAuctioneerService _auctioneerService;

        public AuctioneerController(IAuctioneerService auctioneerService)
        {
            _auctioneerService = auctioneerService;
        }

        [HttpPost]
        [Route("lot")]
        [ProducesResponseType(typeof(EditedLotResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> Post(dynamic request, CancellationToken cancellationToken)
        {
            EditedLotResponse lotResponse = await _auctioneerService.InsertWithRetryAsync(request.ToString(), cancellationToken);


            return Ok(lotResponse);
        }

        [HttpGet]
        [Route("lot")]
        [ProducesResponseType(typeof(EditedLotResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> Get(long auctionId, long lotId)
        {
            var lotResponse = await _auctioneerService.GetAsync(auctionId, lotId);
            return Ok(lotResponse);
        }

        [HttpGet]
        [Route("lotbyid")]
        [ProducesResponseType(typeof(EditedLotResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> Get(string documentId, long auctionId, long lotId)
        {
            var lotResponse = await _auctioneerService.GetByIdAsync(documentId, auctionId, lotId);
            return Ok(lotResponse);
        }

        [HttpDelete]
        [Route("lot")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete(long auctionId, long lotId, CancellationToken cancellationToken)
        {
            await _auctioneerService.DeleteWithRetryAsync(auctionId, lotId, cancellationToken);

            return Ok();
        }

        [HttpDelete]
        [Route("lotbypartitionkey")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteByPartitionKey(long auctionId, long lotId, CancellationToken cancellationToken)
        {
            await _auctioneerService.DeleteByPartitionKeyWithRetryAsync(auctionId, lotId, cancellationToken);

           

            return Ok();
        }

        
        /// <summary>
        /// Validate and transform LotDetail API
        /// </summary>
        /// <param name="request">Validate LotDetail request</param>
        /// <returns>Return lot response if LotDetail is valid</returns>
        [HttpPost]
        [Route("validateandtransformlotdetail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ValidateAndTransformLotDetail([FromBody] dynamic request)
        {
            LotResponse lotResponse = await _auctioneerService.ValidateAndTransformLotDetail(request);
            return Ok(lotResponse);
        }

        /// <summary>
        /// Edit Lot API
        /// </summary>
        /// <param name="request">When auctioneer places a request to edit lot</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Return LotResponse if lot is valid</returns>
        [HttpPost]
        [Route("updatelot")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateLot([FromBody] LotRequest request, CancellationToken cancellationToken)
        {
            (object editedLotResponse,LotDetail lotDetail) = await _auctioneerService.UpdateWithRetryAsync(request, cancellationToken);

         

            return Ok(editedLotResponse);
        }
    }
}