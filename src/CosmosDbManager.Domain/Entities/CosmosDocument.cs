using System.Text.Json;

namespace CosmosDbManager.Domain.Entities;

public sealed record CosmosDocument
{
    public CosmosDocument(
        string id,
        string partitionKeyValue,
        JsonElement payload,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id cannot be null or whitespace.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(partitionKeyValue))
        {
            throw new ArgumentException("Partition key value cannot be null or whitespace.", nameof(partitionKeyValue));
        }

        Id = id.Trim();
        PartitionKeyValue = partitionKeyValue.Trim();
        Payload = payload.Clone();
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public string Id { get; }

    public string PartitionKeyValue { get; }

    public JsonElement Payload { get; }

    public DateTimeOffset? CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; }
}
