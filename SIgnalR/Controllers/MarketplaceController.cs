using Microsoft.AspNetCore.Mvc;
using SIgnalR.Model;
using SIgnalR.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SIgnalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketplaceController : ControllerBase
    {
        private readonly IMessageHandler _messageHandler;

        public MarketplaceController(IMessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
        }

        // POST api/<NotifyController>
        [HttpPost]
        public void NotiFyClients(Response res)
        {
            
            //if (res.Topic !="General")
            //{ 
            //    _messageHandler.NotiFy(res); 
            //}
            //else {
            //    _messageHandler.BroadcastMessage(res); 
            //}
        }
       
    }
}
