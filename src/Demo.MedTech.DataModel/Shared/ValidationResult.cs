using System.Collections.Generic;

namespace Demo.MedTech.DataModel.Shared
{
    public class ValidationResult
    {
        public ValidationResult()
        {
        }

        public ValidationResult(int code, string value, string description, int? priority = null)
        {
            Code = code;
            Value = value;
            Description = description;
            Priority = priority;
        }

        public int Code { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }

        public int? Priority { get; set; }

        public Dictionary<string, object> Data { get; set; }
    }
}