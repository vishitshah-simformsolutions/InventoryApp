using Microsoft.AspNetCore.Http;
using SIgnalR.Extensions;
using SIgnalR.Model;
using SIgnalR.Service.IService;
using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SIgnalR.Service
{
    public class PlaceBidHandler : IPlaceBid
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageHandler _messageHandler;
        private readonly IServiceBusHelper _serviceBusHelper;
        private readonly IConfiguration _configuration;

        public PlaceBidHandler(HttpClient client, IHttpContextAccessor httpContextAccessor, IMessageHandler messageHandler, IServiceBusHelper serviceBusHelper, IConfiguration configuration)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
            _messageHandler = messageHandler;
            _serviceBusHelper = serviceBusHelper;
            _configuration = configuration;
        }

        public async Task<string> PlaceBid(BidRequest reqForBid)
        {
            _client.DefaultRequestHeaders.Add("x-atg-client-id", Convert.ToString(_httpContextAccessor.HttpContext.Request.Headers["x-atg-client-id"]));
            _client.DefaultRequestHeaders.Add("x-atg-client-ip", Convert.ToString(_httpContextAccessor.HttpContext.Request.Headers["x-atg-client-ip"]));
            _client.DefaultRequestHeaders.Add("x-atg-app-id", Convert.ToString(_httpContextAccessor.HttpContext.Request.Headers["x-atg-app-id"]));
            _client.DefaultRequestHeaders.Add("x-atg-user-id", Convert.ToString(_httpContextAccessor.HttpContext.Request.Headers["x-atg-user-id"]));

            _client.DefaultRequestHeaders.Add("ocp-apim-subscription-key", _configuration["OCP_APIM_KEY"]);

            var response = await _client.PostAsJson(_configuration["ApiConfigs:Bid:EndPoint"], reqForBid);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };

            var bidResponseJson = await response.Content.ReadAsStringAsync();
            var bidResponse = JsonSerializer.Deserialize<BidResponse>(bidResponseJson, options);

            if (bidResponse.isValid)
            {
                // send message to service bus for sbs egress
                var lastBiddingStates = bidResponse.biddingStates?.LastOrDefault()?.state;

                dynamic result = new ExpandoObject();
                result.timeStamp = bidResponse.timeStamp.ToString(CultureInfo.InvariantCulture);
                result.bidderId = lastBiddingStates?.bidderId;
                result.endTime = lastBiddingStates?.endTime;

                return JsonSerializer.Serialize(result);
            }
            else
            {
                return JsonSerializer.Serialize(new { bidResponse.timeStamp, bidResponse.validationResults });
            }

        }
    }
}

