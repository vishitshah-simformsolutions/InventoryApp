﻿namespace Product.DAL.Cosmos
{
    public class CosmosLotDocument 
    {
        public string id { get; set; }
        public string PartitionKey { get; set; } = "QA";
        public byte[] EncodedProductModel { get; set; }
    }
}