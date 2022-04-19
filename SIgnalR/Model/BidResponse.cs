using System;
using System.Collections.Generic;

namespace SIgnalR.Model
{
    public class BidResponse : LotModel
    {
        public DateTime timeStamp { get; set; }
        public string requestId { get; set; }
        public bool isValid { get; set; } = true;
        public List<ValidationResult> validationResults { get; set; }
        public InferredData inferredData { get; set; }
    }

    public class ValidationResult
    {
        public int code { get; set; }
        public string value { get; set; }
        public string description { get; set; }
    }

    public class InferredData
    {
        public string outBidderId { get; set; }
    }

}
