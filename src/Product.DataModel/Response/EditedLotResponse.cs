﻿using System;
using System.Collections.Generic;
using Product.DataModel.Shared;

namespace Product.DataModel.Response
{
    public class EditedLotResponse : ProductModel, IResponse, IRuleValidationMessage
    {
        public DateTime TimeStamp { get; set; }
        public string RequestId { get; set; }
        public bool IsValid { get; set; } = true;
        public List<ValidationResult> ValidationResults { get; set; } = new List<ValidationResult>();
        public string SchemaType { get; set; }
    }
}