using CosmosDbManager.Infrastructure.Configuration;
using CosmosDbManager.Infrastructure.Resilience;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging.Abstractions;

namespace CosmosDbManager.Infrastructure.Tests.Infrastructure;

public sealed class RetryPolicyHelperTests
{
    [Fact]
    public async Task CreateCosmosRetryPolicy_WhenTransientCosmosExceptionOccurs_RetriesAndSucceeds()
    {
        var settings = new CosmosDbSettings
        {
            MaxRetryAttempts = 3,
            RequestTimeoutSeconds = 10
        };

        var policy = RetryPolicyHelper.CreateCosmosRetryPolicy(settings, NullLogger.Instance);

        var attempts = 0;
        var result = await policy.ExecuteAsync(() =>
        {
            attempts++;

            if (attempts < 3)
            {
                throw new CosmosException("transient", System.Net.HttpStatusCode.ServiceUnavailable, 0, "activity-id", 0.1);
            }

            return Task.FromResult("ok");
        });

        Assert.Equal("ok", result);
        Assert.Equal(3, attempts);
    }
}
