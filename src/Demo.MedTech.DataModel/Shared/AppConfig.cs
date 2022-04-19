using System.Diagnostics.CodeAnalysis;

namespace Demo.MedTech.DataModel.Shared
{
    [ExcludeFromCodeCoverage]
    public class Settings
    {
        public int InMemoryCacheExpiryInSeconds { get; set; }
    }

    public class SbsConfigurationOptions
    {
        public string CosmosDbEndpoint { get; set; }
        public string CosmosDbMasterKey { get; set; }
        public string CosmosDdContainerName { get; set; }
        public string CosmosDatabase { get; set; }
    }
}