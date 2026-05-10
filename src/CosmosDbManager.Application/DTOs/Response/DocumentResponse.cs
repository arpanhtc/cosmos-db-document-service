namespace CosmosDbManager.Application.DTOs.Response;

public sealed class DocumentResponse
{
    public string Id { get; set; } = string.Empty;

    public string PartitionKeyValue { get; set; } = string.Empty;

    public string JsonPayload { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }
}
