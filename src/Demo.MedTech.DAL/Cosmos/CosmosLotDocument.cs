namespace Demo.MedTech.DAL.Cosmos
{
    public class CosmosLotDocument 
    {
        public string id { get; set; }
        public string PartitionKey { get; set; } = "QA";
        public byte[] EncodedLotModel { get; set; }
    }
}