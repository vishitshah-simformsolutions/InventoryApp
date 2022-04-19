using Product.DataModel.Shared;
using Product.Utility.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Product.DataModel.Exceptions;

namespace Product.ValidationEngine.Model
{
    public class Config
    {
        private const int ConfigContextErrorCode = 116;

        #region Properties

        private static List<PlatformConfig> PlatformRules { get; }
        private static List<RuleType> DefaultLotRuleGroup { get; }
        private static List<RuleType> DefaultAuctionRuleGroup { get; }
        public List<RuleType> AuctionRuleGroup { get; }
        public List<RuleType> LotRuleGroup { get; }

        public static Dictionary<string, bool> DynamicAuditLogRules { get; }
        public static Dictionary<string, string> ErrorDescriptions { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// This constructor do file read and serialization operation and throw exception if any
        /// </summary>
        static Config()
        {
            var validationResult = new RuleValidationMessage() { IsValid = true };
            try
            {
               //Reading file and serialize object of PlatformConfig
                PlatformRules = new List<PlatformConfig>();
                var jsonPlatformConfig =
                    File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ProductPlatformConfiguration.json"));
                PlatformRules = JsonSerializer.Deserialize<List<PlatformConfig>>(jsonPlatformConfig,
                    JsonSerializerOption.CaseInsensitive);

                if (!PlatformRules.Any())
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationResults.AddRange(
                        Response.ValidationResults.Where(x => x.Code == ConfigContextErrorCode));
                    throw new RuleEngineException(validationResult);
                }

                var platformConfig = PlatformRules.FirstOrDefault(x => x.PlatformCode == "0");
                if (platformConfig != null)
                {
                    DefaultLotRuleGroup = platformConfig.LotRuleGroup;
                    DefaultAuctionRuleGroup = platformConfig.AuctionRuleGroup;
                }
                else
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationResults.AddRange(
                        Response.ValidationResults.Where(x => x.Code == ConfigContextErrorCode));
                    throw new RuleEngineException(validationResult);
                }
            }
            catch (Exception)
            {
                validationResult.IsValid = false;
                validationResult.ValidationResults.AddRange(Response.ValidationResults.Where(x => x.Code == ConfigContextErrorCode));
                throw new RuleEngineException(validationResult);
            }
        }

        /// <summary>
        /// Assign rule group when context is initiated
        /// </summary>
        /// <param name="platformCode">Platform is passed by user</param>
        public Config(string platformCode)
        {
            if (PlatformRules.Any(x => x.PlatformCode == platformCode))
            {
                var platformGroup = PlatformRules.FirstOrDefault(x => x.PlatformCode == platformCode);
                LotRuleGroup = platformGroup?.LotRuleGroup;
                AuctionRuleGroup = platformGroup?.AuctionRuleGroup;
            }
            else
            {
                LotRuleGroup = DefaultLotRuleGroup;
                AuctionRuleGroup = DefaultAuctionRuleGroup;
            }
        }

        #endregion
    }
}