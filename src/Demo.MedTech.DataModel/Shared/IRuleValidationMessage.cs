using System;
using System.Collections.Generic;

namespace Demo.MedTech.DataModel.Shared
{
    public interface IRuleValidationMessage
    {
        public DateTime TimeStamp { get; set; }

        public string RequestId { get; set; }

        public bool IsValid { get; set; }

        public List<ValidationResult> ValidationResults { get; set; }
    }
}