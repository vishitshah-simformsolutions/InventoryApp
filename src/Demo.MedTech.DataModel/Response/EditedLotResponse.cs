using System;
using System.Collections.Generic;
using Demo.MedTech.DataModel.Shared;

namespace Demo.MedTech.DataModel.Response
{
    public class EditedLotResponse : LotModel, IResponse, IRuleValidationMessage
    {
        public DateTime TimeStamp { get; set; }
        public string RequestId { get; set; }
        public bool IsValid { get; set; } = true;
        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
        public string SchemaType { get; set; }
    }
}