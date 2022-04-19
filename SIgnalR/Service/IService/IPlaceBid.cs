using System.Threading.Tasks;
using SIgnalR.Model;

namespace SIgnalR.Service.IService
{
    public interface IPlaceBid
    {
        Task<string> PlaceBid(BidRequest reqForBid);        
    }
}
