using System;
using Product.DataModel.Shared;

namespace Product.DataModel.Exceptions
{
    public class RuleEngineException : Exception
    {
        public RuleValidationMessage RuleValidationMessage { get; }

        public Exception BaseException { get; }

        public RuleEngineException(RuleValidationMessage ruleValidationMessage, Exception baseException)
            : base(nameof(RuleEngineException), baseException)
        {
            RuleValidationMessage = ruleValidationMessage;
            BaseException = baseException;
        }

        public RuleEngineException(RuleValidationMessage ruleValidationMessage)
            : base(nameof(RuleEngineException))
        {
            RuleValidationMessage = ruleValidationMessage;
        }
    }
}