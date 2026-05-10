using CosmosDbManager.Domain.ValueObjects;

namespace CosmosDbManager.Application.Tests.Domain.ValueObjects;

public sealed class CosmosConfigurationTests
{
    [Fact]
    public void Constructor_ValidValues_InitializesConfiguration()
    {
        var configuration = new CosmosConfiguration(
            connectionString: "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;",
            databaseName: "MyDatabase",
            containerName: "MyContainer",
            partitionKey: "/tenantId");

        Assert.Equal("AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;", configuration.ConnectionString);
        Assert.Equal("MyDatabase", configuration.DatabaseName);
        Assert.Equal("MyContainer", configuration.ContainerName);
        Assert.Equal("/tenantId", configuration.PartitionKey);
    }

    [Fact]
    public void Constructor_BlankContainerName_ThrowsArgumentException()
    {
        var action = () => new CosmosConfiguration(
            connectionString: "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;",
            databaseName: "MyDatabase",
            containerName: " ",
            partitionKey: "/tenantId");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("containerName", exception.ParamName);
    }
}
