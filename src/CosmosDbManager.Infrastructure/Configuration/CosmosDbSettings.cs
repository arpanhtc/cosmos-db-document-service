namespace CosmosDbManager.Infrastructure.Configuration;

public sealed class CosmosDbSettings
{
    public const string SectionName = "CosmosDb";

    public string DefaultConnectionString { get; set; } = string.Empty;

    public string DefaultDatabaseName { get; set; } = string.Empty;

    public string DefaultContainerName { get; set; } = string.Empty;

    public string DefaultPartitionKey { get; set; } = string.Empty;

    public int MaxRetryAttempts { get; set; } = 3;

    public int RequestTimeoutSeconds { get; set; } = 30;

    public string ConnectionMode { get; set; } = "Gateway";
}
