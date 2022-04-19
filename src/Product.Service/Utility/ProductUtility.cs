using Product.DataModel.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Product.Utility
{
    public static class ProductUtility
    {
        public static JsonProperty GetPropertyCaseInsensitive(JsonElement jsonElement, string propName)
        {
            return jsonElement.EnumerateObject().FirstOrDefault(p => string.Compare(p.Name, propName,
                StringComparison.OrdinalIgnoreCase) == 0);
        }
       
        public static ProductModel AddDefaultState(dynamic lotDetail, ProductModel lotModel, string correlationId)
        {
            return new ProductModel
            {
                ProductDetail = lotModel.ProductDetail
            };
        }
    }
}
