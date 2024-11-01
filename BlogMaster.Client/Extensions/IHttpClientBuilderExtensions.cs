using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace BlogMaster.Client.Extensions
{
    public static class IHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddCustomResilienceHandler(this IHttpClientBuilder builder)
        {
            var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 10,
                BackoffType = DelayBackoffType.Linear,
                Delay = TimeSpan.FromMilliseconds(100),
                MaxDelay = TimeSpan.FromMilliseconds(1000),
                ShouldHandle = response =>
                {
                    var outcome = response.Outcome;
                    return ValueTask.FromResult(outcome.Exception != null || (outcome.Result?.IsSuccessStatusCode == false));
                }
            };

            builder.AddResilienceHandler("CustomResilience", pipeline => 
            {
                pipeline.AddRetry(retryOptions);
            });
            return builder;
        }
    }
}
