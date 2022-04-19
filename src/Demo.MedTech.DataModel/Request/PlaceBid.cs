namespace Demo.MedTech.DataModel.Request
{
    public class PlaceBid
    {
        public long LotId { get; set; }

        public long AuctionId { get; set; }

        public string PlatformCode { get; set; }
       
        public string MarketplaceCode { get; set; }

        public string BidderId { get; set; }

        public decimal Amount { get; set; }

        public string BidderRef { get; set; }

        public string MarketplaceChannelCode { get; set; }
    }
}