using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Product.DataModel.Shared
{
    public class ProductModel
    {
        public ProductDetail ProductDetail { get; set; }
        [JsonIgnore]
        public string ETag { get; set; }
    }


    public class Increment
    {
        public decimal Low { get; set; }
        public decimal? High { get; set; }
        public decimal? IncrementValue { get; set; }
    }
    public class ProductDetail
    {
        public long ProductId { get; set; }
        public long ItemId { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? ManufacturingPrice { get; set; }
        public List<Increment> Increment { get; set; }
        public decimal? Quantity { get; set; }
    }
   

}