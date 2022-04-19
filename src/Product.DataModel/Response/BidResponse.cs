using System;
using System.Collections.Generic;
using Product.DataModel.Shared;

namespace Product.DataModel.Response
{
    public class BidMinimumAndStandardResponse : IResponse, IRuleValidationMessage
    {
        public DateTime TimeStamp { get; set; }
        public string RequestId { get; set; }
        public bool IsValid { get; set; } = true;
        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
        public string SchemaType { get; set; }
        public long AuctionId { get; set; }
        public long LotId { get; set; }
        public List<BiddingState> BiddingStates { get; set; }
    }
}