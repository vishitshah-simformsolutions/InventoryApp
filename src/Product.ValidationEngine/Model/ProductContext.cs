using Product.DataModel.Shared;
using Product.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Product.DataModel.Exceptions;
using Product.ValidationEngine.Rules;

namespace Product.ValidationEngine.Model
{
    public class ProductContext
    {
        private readonly IEnumerable<IRule> _rules;
        private readonly IEnumerable<ITransform> _transformationRules;
        public IDictionary<string, JsonElement> productDetailRequestDictionary = new Dictionary<string, JsonElement>();
        private const int UnknownErrorCode = 999;

        #region Properties

        public Config Config { get; }
        public ProductDetail ProductDetail { get; }
        public bool validateProductDetail { get; }

        private RuleValidationMessage _ruleValidationMessage;
        private List<ValidationResult> _validationLogs;

        #endregion

        #region Constructor

        static ProductContext()
        {
        }

        /// <summary>
        /// Accepts json string for Lot Detail class, validates it and assigns it to the ProductDetail property of this static class
        /// </summary>
        /// <param name="productDetailJson">Json request for ProductDetail</param>
        /// <param name="validateProductDetail">Boolean flag for auction detail call</param>
        /// <param name="requestPipe"></param>
        /// <param name="rules"></param>
        /// <param name="transformationRules"></param>
        /// <param name="platformCode"></param>
        public ProductContext(string productDetailJson, IRequestPipe requestPipe,
            IEnumerable<IRule> rules, IEnumerable<ITransform> transformationRules, bool validateProductDetail = false, string platformCode = "0")
        {
            _rules = rules;
            _transformationRules = transformationRules;

            if (string.IsNullOrEmpty(productDetailJson))
            {
                throw new ArgumentNullException(nameof(productDetailJson));
            }

            ValidateAllMandatoryData(productDetailJson, validateProductDetail);
            Config = new Config(platformCode);

            ProductDetail = new ProductDetailRequest(productDetailRequestDictionary, validateProductDetail);
            validateProductDetail = validateProductDetail;
        }
        #endregion

        /// <summary>
        /// Validate request for missing data
        /// </summary>
        /// <param name="productDetailJson">Json request from api</param>
        /// <param name="validateProductDetail">Boolean flag for auction detail call</param>
        /// <returns>RuleValidationMessage</returns>
        private void ValidateAllMandatoryData(string productDetailJson, bool validateProductDetail = false)
        {
            var ruleValidationMessage = new RuleValidationMessage { IsValid = true };

            productDetailRequestDictionary =
                JsonSerializer
                    .Deserialize<IDictionary<string, JsonElement>>(productDetailJson)
                    .ToDictionary(k => k.Key.ToLower(), k => k.Value);

            if (productDetailRequestDictionary == null)
            {
                ruleValidationMessage.IsValid = false;
                ruleValidationMessage.ValidationResults.AddRange(Response.ValidationResults.Where(x => x.Code == UnknownErrorCode));
                throw new RuleEngineException(ruleValidationMessage);
            }
        }


        /// <summary>
        /// Extension method to evaluate validation rules
        /// </summary>
        public async Task<RuleValidationMessage> EvaluateAsync()
        {
            _ruleValidationMessage = new RuleValidationMessage { IsValid = true };
            _validationLogs = new List<ValidationResult>();

            var ruleGroup = Config.ProductRuleGroup;

            foreach (var rulePair in ruleGroup)
            {
                foreach (var softRule in rulePair.SoftRules.Where(x => x.Value))
                {
                    ExecuteRule(softRule.Key, nameof(RuleType.SoftRules), rulePair.RuleExecutionType);
                }

                foreach (var hardRule in rulePair.HardRules.Where(x => x.Value))
                {
                    ExecuteRule(hardRule.Key, nameof(RuleType.HardRules), rulePair.RuleExecutionType);
                }

                //This is to break the out most foreach loop if there is any invalid result rule in current rule group
                if (!_ruleValidationMessage.IsValid)
                {
                    break;
                }
            }

            if (!_ruleValidationMessage.IsValid)
            {
                throw new RuleEngineException(_ruleValidationMessage);
            }

            return _ruleValidationMessage;
        }

        /// <summary>
        /// Create rule class object and execute rule
        /// </summary>
        /// <param name="ruleName">RuleName</param>
        /// <param name="ruleType">RuleType</param>
        /// <param name="ruleExecutionType"></param>
        private void ExecuteRule(string ruleName, string ruleType, string ruleExecutionType)
        {
            RuleValidationMessage result;
            if (ruleExecutionType == "Transformation")
            {
                var rule = _transformationRules.FirstOrDefault(r => r.GetType().Name == ruleName);
                // Perform evaluation on provided object
                if (rule == null)
                {
                    return;
                }

                result = rule.Transform(this);
            }
            else
            {
                var rule = _rules.FirstOrDefault(r => r.GetType().Name == ruleName);
                // Perform evaluation on provided object
                if (rule == null)
                {
                    return;
                }

                result = rule.Execute(this);
            }

            if (result == null || result.ValidationResults.Count <= 0)
            {
                return;
            }

            _ruleValidationMessage.ValidationResults.AddRange(result.ValidationResults);

           if (ruleType == nameof(RuleType.HardRules) && !result.IsValid)
            {
                _ruleValidationMessage.IsValid = result.IsValid;
            }
        }
    }
}