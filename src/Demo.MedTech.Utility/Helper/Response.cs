using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Demo.MedTech.DataModel.Exceptions;
using Demo.MedTech.DataModel.Shared;

namespace Demo.MedTech.Utility.Helper
{
    // TODO: Rename this class and file
    public static class Response
    {
        #region Constructor

        /// <summary>
        /// This constructor do file read and serialization operation and throw exception if any
        /// </summary>
        static Response()
        {
            try
            {
                string json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "StatusCodes.json"));

                ValidationResults = JsonSerializer.Deserialize<List<ValidationResult>>(json, JsonSerializerOption.CaseInsensitive);
            }
            catch (Exception e)
            {
                var ruleValidationMessage = new RuleValidationMessage()
                {
                    IsValid = false,
                    ValidationResults = new List<ValidationResult>()
                    {
                        new ValidationResult(901, "ERROR_STATUS_CONTEXT", "Error in reading or serialization Response.ValidationResult")
                    }
                };
                throw new RuleEngineException(ruleValidationMessage, e);
            }
        }

        #endregion

        public static List<ValidationResult> ValidationResults { get; set; }

        /// <summary>
        /// Prepare validation result by replacing description with dynamic message
        /// </summary>
        /// <param name="statusCode">StatusCode of validation results</param>
        /// <param name="placeHolders">placeholder value for replacement</param>
        /// <returns></returns>
        public static ValidationResult PrepareValidationResult(int statusCode, params string[] placeHolders)
        {
            var validationResult = new ValidationResult();
            var message = ValidationResults.FirstOrDefault(x => x.Code == statusCode);

            if (message == null)
            {
                return validationResult;
            }

            validationResult.Code = message.Code;
            validationResult.Value = message.Value;
            validationResult.Description = message.Description;
            validationResult.Description = string.Format(validationResult.Description, placeHolders);
            validationResult.Priority = message.Priority;
            return validationResult;
        }
    }
}