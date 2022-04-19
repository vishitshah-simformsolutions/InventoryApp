namespace Playground.Policies
{
    /// <summary>
    /// Config values related to Cosmos retry policy
    /// </summary>
    public class CosmosPollySettings
    {
        public int RetryTimeInSeconds { get; set; }
        public int TimeoutPeriodInSeconds { get; set; }
        public int RetryCount { get; set; }
    }
}
