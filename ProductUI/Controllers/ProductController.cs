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
        public ActionResult VerifyProduct(ProductDetail productDetails)
        {
            productDetails.Increment = new List<Increment>();
            var increment = new Increment
            {
                Low = 0,
                High = null,
                IncrementValue = 10
            };
            productDetails.Increment.Add(increment);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", JsonSerializer.Serialize(productDetails), ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["API"] + _configuration["VERIFY_PRODUCT_ENDPOINT"]);
            return Ok(response.Content);
        }

        [HttpPost]
        public ActionResult CreateProduct(ProductDetail productDetails)
        {
            productDetails.Increment = new List<Increment>();
            var increment = new Increment
            {
                Low = 0,
                High = null,
                IncrementValue = 10
            };
            productDetails.Increment.Add(increment);
            var request = new RestRequest(Method.POST);
            StringBuilder correlation = new StringBuilder();

            string correlationId = correlation.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
            .Append("SBS")
            .Append("PLAY")
            .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
            .ToString();

            request.AddHeader("x-correlation-id", correlationId);
            request.AddParameter("application/json", JsonSerializer.Serialize(CreateProductObj(productDetails)),
                ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["API"] + _configuration["PRODUCT_DETAILS_ENDPOINT"]);


            return Ok(response.Content == "" ? response?.ErrorException?.Message : response.Content);
        }

        [HttpPut]
        public ActionResult UpdateProduct(ProductDetail productDetails)
        {
            productDetails.Increment = new List<Increment>();
            var increment = new Increment
            {
                Low = 0,
                High = null,
                IncrementValue = 10
            };
            productDetails.Increment.Add(increment);
            var request = new RestRequest(Method.POST);
            StringBuilder correlation = new StringBuilder();

            string correlationId = correlation.Append(DateTimeOffset.Now.ToUnixTimeSeconds())
                .Append("SBS")
                .Append("PLAY")
                .Append(Guid.NewGuid().ToString("N").Substring(0, 15))
                .ToString();

            request.AddHeader("x-correlation-id", correlationId);
            request.AddParameter("application/json", JsonSerializer.Serialize(CreateProductObj(productDetails)),
                ParameterType.RequestBody);

            IRestResponse response = _restClientApiCall.Execute(request, _configuration["API"] + _configuration["LOT_UPDATE_DETAILS_ENDPOINT"]);

            return Ok(response.Content == "" ? response?.ErrorException?.Message : response.Content);
        }

        [HttpDelete]
        public ActionResult DeleteProduct(long itemId, long productId)
        {
            string url = _configuration["API"] + _configuration["DELETE_PRODUCT_ENDPOINT"] + $"?itemId={itemId}&productId={productId}";
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

        private ProductResponseModel CreateProductObj(ProductDetail productDetails)
        {
            return new ProductResponseModel()
            {
                Domain = "SBS",
                SubDomain = "Product",
                LotId = productDetails.ItemId,
                ProductId = productDetails.ProductId,
                ProductDetail = productDetails,
            };
        }
    }
}
