namespace CosmosDbManager.Domain.ValueObjects;

public sealed record CosmosConfiguration
{
    public CosmosConfiguration(
        string connectionString,
        string databaseName,
        string containerName,
        string partitionKey)
    {
        ConnectionString = EnsureRequired(connectionString, nameof(connectionString));
        DatabaseName = EnsureRequired(databaseName, nameof(databaseName));
        ContainerName = EnsureRequired(containerName, nameof(containerName));
        PartitionKey = EnsureRequired(partitionKey, nameof(partitionKey));
    }

    public string ConnectionString { get; }

    public string DatabaseName { get; }

    public string ContainerName { get; }

    public string PartitionKey { get; }

    private static string EnsureRequired(string value, string name)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be null or whitespace.", name)
            : value.Trim();
    }
}
