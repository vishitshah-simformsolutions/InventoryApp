using System.Collections.Generic;

namespace Product.ValidationEngine.Model
{
    public class PlatformConfig
    {
        public string PlatformCode { get; set; }
        public List<RuleType> ProductRuleGroup { get; set; }
    }

    public class RuleType
    {
        public string RuleExecutionType { get; set; }
        public Dictionary<string, bool> SoftRules { get; set; }
        public Dictionary<string, bool> HardRules { get; set; }
    }
}