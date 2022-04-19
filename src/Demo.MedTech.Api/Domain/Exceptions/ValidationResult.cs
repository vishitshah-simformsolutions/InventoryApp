using System.Runtime.Serialization;

namespace Demo.MedTech.Api.Domain.Exceptions
{
    [DataContract]
    public class ValidationResult
    {
        [DataMember(Name = "error_code")] public string ErrorCode { get; private set; }

        [DataMember(Name = "error_msg")] public string ErrorMessage { get; private set; }

        [DataMember(Name = "error_field")] public string ErrorField { get; private set; }

        public ValidationResult(string errorCode, string errorMessage, string errorField = null)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            ErrorField = errorField;
        }
    }
}