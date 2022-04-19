namespace SIgnalR.Model
{
    public class BidRequest
    {
        public long atgauctionid { get; set; }
        public long atglotid { get; set; }
        public string atgplatformcode { get; set; }
        public string atgmarketplacecode { get; set; }
        public string bidderid { get; set; }
        public int amount { get; set; }
    }
}
