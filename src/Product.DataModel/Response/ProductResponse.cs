using System;
using System.Collections.Generic;
using Product.DataModel.Shared;

namespace Product.DataModel.Response
{
    public class ProductResponse : IRuleValidationMessage
    {
        public DateTime TimeStamp { get; set; }

        public string RequestId { get; set; }

        public bool IsValid { get; set; } = true;

        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();

        public ProductDetail ProductDetail { get; set; }
    }
}
