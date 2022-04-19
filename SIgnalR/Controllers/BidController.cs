using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIgnalR.Model;
using SIgnalR.Service.IService;
using System;
using System.Threading.Tasks;

namespace SIgnalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly IPlaceBid _bidService;

        public BidController(IPlaceBid bidService)        
        {           
            _bidService = bidService;
        }

        [HttpPost]
        public async Task<ActionResult> Post(BidRequest bidRequest)
        {
            try
            {                
                return Ok(await _bidService.PlaceBid(bidRequest));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
