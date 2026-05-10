namespace CosmosDbManager.Application.DTOs.Request;

public sealed class PatchDocumentRequest
{
    public CosmosConfigurationDto? Configuration { get; set; }

    public string Id { get; set; } = string.Empty;

    public string PartitionKeyValue { get; set; } = string.Empty;

    public List<PatchOperationDto> Operations { get; set; } = [];
}
