namespace SIgnalR.Service.IService
{
    public interface IMessageHandler
    {
        public void BroadcastMessage(string message);
        
        public void UpdateBidResponse(string message);
    }
}
