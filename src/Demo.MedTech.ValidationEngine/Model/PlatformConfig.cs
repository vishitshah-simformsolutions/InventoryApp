using System.Collections.Generic;

namespace Demo.MedTech.ValidationEngine.Model
{
    public class PlatformConfig
    {
        public string PlatformCode { get; set; }
        public List<RuleType> AuctionRuleGroup { get; set; }
        public List<RuleType> LotRuleGroup { get; set; }
    }

    public class RuleType
    {
        public string RuleExecutionType { get; set; }
        public Dictionary<string, bool> SoftRules { get; set; }
        public Dictionary<string, bool> HardRules { get; set; }
    }
}