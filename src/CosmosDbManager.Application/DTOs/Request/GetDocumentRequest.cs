namespace CosmosDbManager.Application.DTOs.Request;

public sealed class GetDocumentRequest
{
    public CosmosConfigurationDto? Configuration { get; set; }

    public string Id { get; set; } = string.Empty;

    public string PartitionKeyValue { get; set; } = string.Empty;
}
