namespace CosmosDbManager.Application.DTOs.Request;

public sealed class CosmosConfigurationDto
{
    public string ConnectionString { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;

    public string ContainerName { get; set; } = string.Empty;

    public string PartitionKey { get; set; } = string.Empty;
}
