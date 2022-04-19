using Playground.Services.IServices;
using RestSharp;

namespace Playground.Services
{
    public class RestClientApiCall : IRestClientApiCall
    {
        public IRestResponse Execute(IRestRequest request, string URL)
        {
            var client = new RestClient(URL)
            {
                Timeout = -1
            };

            IRestResponse response = client.Execute(request);

            return response;
        }
    }
}
