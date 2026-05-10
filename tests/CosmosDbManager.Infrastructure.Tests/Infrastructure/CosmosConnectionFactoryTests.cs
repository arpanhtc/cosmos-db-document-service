using CosmosDbManager.Domain.ValueObjects;
using CosmosDbManager.Infrastructure.Configuration;
using CosmosDbManager.Infrastructure.CosmosDb;
using Microsoft.Extensions.Options;

namespace CosmosDbManager.Infrastructure.Tests.Infrastructure;

public sealed class CosmosConnectionFactoryTests
{
    [Fact]
    public void CreateClient_WithSameConnectionString_ReturnsCachedInstance()
    {
        var factory = new CosmosConnectionFactory(Options.Create(new CosmosDbSettings
        {
            RequestTimeoutSeconds = 45,
            MaxRetryAttempts = 4,
            ConnectionMode = "Gateway"
        }));

        var configuration = new CosmosConfiguration(
            "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VR8yCw==;",
            "db1",
            "container1",
            "/tenantId");

        var client1 = factory.CreateClient(configuration);
        var client2 = factory.CreateClient(configuration);

        Assert.Same(client1, client2);
    }
}
