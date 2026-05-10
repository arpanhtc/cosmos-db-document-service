using CosmosDbManager.Infrastructure.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Polly;

namespace CosmosDbManager.Infrastructure.Resilience;

public static class RetryPolicyHelper
{
    public static IAsyncPolicy CreateCosmosRetryPolicy(CosmosDbSettings settings, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logger);

        return Policy
            .Handle<CosmosException>(IsTransient)
            .WaitAndRetryAsync(
                retryCount: settings.MaxRetryAttempts,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100)
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetry: (exception, delay, attempt, _) =>
                {
                    logger.LogWarning(
                        exception,
                        "Retry {Attempt}/{MaxAttempts} after {DelayMs}ms.",
                        attempt,
                        settings.MaxRetryAttempts,
                        delay.TotalMilliseconds);
                });
    }

    private static bool IsTransient(CosmosException exception)
    {
        return exception.StatusCode is
            System.Net.HttpStatusCode.TooManyRequests or
            System.Net.HttpStatusCode.RequestTimeout or
            System.Net.HttpStatusCode.ServiceUnavailable;
    }
}
