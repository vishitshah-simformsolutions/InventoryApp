using System;
using System.Collections.Generic;

namespace Playground.Models
{
    public class EgressBidHistory
    {
        public string Id { get; set; }
        public long AuctionId { get; set; }
        public long LotId { get; set; }
        public int SequenceNumber { get; set; }
        public string BidderId { get; set; }
        public decimal BidderAmount { get; set; }
        public DateTime BidDateTime { get; set; }
        public string PlatformCode { get; set; }
        public bool IsTemporary { get; set; }
    }

    public class EgressBidHistoryList
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public List<EgressBidHistory> BidHistory { get; set; }
    }
}