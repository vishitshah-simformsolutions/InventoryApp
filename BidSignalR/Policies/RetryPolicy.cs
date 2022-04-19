using Microsoft.Azure.Cosmos;
using Polly;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Net;

namespace Playground.Policies
{
    public static class RetryPolicy
    {
        public static AsyncPolicyWrap GetCosmosAsyncPolicy(int retryTimeInSeconds = 10, int retryCount = 3, int timeoutInSeconds = 3000)
        {
            var overallTimeoutPolicy = Policy.TimeoutAsync(timeoutInSeconds, TimeoutStrategy.Optimistic);
            var waitAndRetryPolicy = Policy
                .Handle<CosmosException>(x => x.StatusCode == HttpStatusCode.PreconditionFailed || x.StatusCode == HttpStatusCode.TooManyRequests || x.StatusCode == HttpStatusCode.Conflict)
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(retryTimeInSeconds));

            return overallTimeoutPolicy.WrapAsync(waitAndRetryPolicy);
        }
    }
}
