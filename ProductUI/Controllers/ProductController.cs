using Product.DataModel.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Playground.Models;
using Playground.Policies;
using Playground.Services.IServices;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Playground.Controllers
{
    public class EgressLotDetail : ProductDetail
    {
        public bool EgressIsPiecemeal
        {
            get => false;
            set {; }
        }
    }

    public class ProductController : Controller
    {
        private readonly IRestClientApiCall _restClientApiCall;
        private static IConfiguration _configuration;
        private readonly CosmosPollySettings _cosmosPollySettings;

        public ProductController(IRestClientApiCall restClientApiCall, IConfiguration configuration, IOptions<CosmosPollySettings> cosmosOptions)
        {
            _restClientApiCall = restClientApiCall;
            _configuration = configuration;
            _cosmosPollySettings = cosmosOptions.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyLot(EgressLotDetail lotDetails)
        {
            lotDetails.Increment = new List<Increment>();
            var increment = new Increment
            {
                Low = 0,
                High = null,
                IncrementValue = 10
            };
            lotDetails.Increment.Add(increment);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", JsonSerializer.Serialize(lotDetails), ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["API"] + _configuration["VERIFY_PRODUCT_ENDPOINT"]);
            return Ok(response.Content);
        }

        [HttpPost]
        public ActionResult CreateLot(EgressLotDetail lotDetails)
        {
            lotDetails.Increment = new List<Increment>();
            var increment = new Increment
            {
                Low = 0,
                High = null,
                IncrementValue = 10
            };
            lotDetails.Increment.Add(increment);
            var request = new RestRequest(Method.POST);
            StringBuilder correlation = new StringBuilder();

            string correlationId = correlation.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
            .Append("SBS")
            .Append("PLAY")
            .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
            .ToString();

            request.AddHeader("x-correlation-id", correlationId);
            request.AddParameter("application/json", JsonSerializer.Serialize(CreateLotObj(lotDetails)),
                ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["API"] + _configuration["PRODUCT_DETAILS_ENDPOINT"]);

            
            return Ok(response.Content == "" ? response?.ErrorException?.Message : response.Content);
        }

        [HttpPut]
        public ActionResult UpdateLot(EgressLotDetail lotDetails)
        {
            lotDetails.Increment = new List<Increment>();
            var increment = new Increment
            {
                Low = 0,
                High = null,
                IncrementValue = 10
            };
            lotDetails.Increment.Add(increment);
            var request = new RestRequest(Method.POST);
            StringBuilder correlation = new StringBuilder();

            string correlationId = correlation.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
                .Append("SBS")
                .Append("PLAY")
                .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
                .ToString();

            request.AddHeader("x-correlation-id", correlationId);
            request.AddParameter("application/json", JsonSerializer.Serialize(CreateLotObj(lotDetails)),
                ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["API"] + _configuration["LOT_UPDATE_DETAILS_ENDPOINT"]);
            
            return Ok(response.Content == "" ? response?.ErrorException?.Message : response.Content);
        }

        [HttpDelete]
        public ActionResult DeleteLot(long auctionId, long lotId)
        {
            string url = _configuration["API"] + _configuration["DELETE_PRODUCT_ENDPOINT"] + $"?auctionId={auctionId}&lotId={lotId}";
            var request = new RestRequest(Method.DELETE);
            StringBuilder correlation = new StringBuilder();

            string correlationId = correlation.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
                .Append("SBS")
                .Append("PLAY")
            .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
            .ToString();

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("x-correlation-id", correlationId);

            IRestResponse response = _restClientApiCall.Execute(request, url);
            
            if (response.Content == "")
            {
                response.Content = "{\"isValid\":true,\"validationResults\":[]}";
            }

            return Ok(response.Content);
        }

        private EgressLotModel CreateLotObj(ProductDetail lotDetails)
        {
            return new EgressLotModel()
            {
                Domain = "SBS",
                SubDomain = "Auctioneer",
                LotId = lotDetails.ItemId,
                AuctionId = lotDetails.ProductId,
                ProductDetail = lotDetails,
            };
        }
    }
}
