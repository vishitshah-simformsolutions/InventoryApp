using Microsoft.AspNetCore.SignalR;
using SIgnalR.Service.IService;

namespace SIgnalR.Service
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IHubContext<ConnectionHub> _hubContext;
        

        public MessageHandler(IHubContext<ConnectionHub> hubContext)
        {
            _hubContext = hubContext;
        }
        
        public void BroadcastMessage(string message)
        {
            //_hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
        
        public void UpdateBidResponse(string message)
        {
            _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
