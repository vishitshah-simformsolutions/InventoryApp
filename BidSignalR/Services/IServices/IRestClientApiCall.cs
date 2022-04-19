using RestSharp;

namespace Playground.Services.IServices
{
    public interface IRestClientApiCall
    {
        IRestResponse Execute(IRestRequest request, string URL);
    }
}
