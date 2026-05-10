using System.Text.Json;
using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.DTOs.Response;
using CosmosDbManager.Domain.Entities;
using CosmosDbManager.Domain.Enums;
using CosmosDbManager.Domain.ValueObjects;

namespace CosmosDbManager.Application.Mappings;

public static class DocumentMapper
{
    public static CosmosConfiguration ToCosmosConfiguration(CosmosConfigurationDto configuration)
    {
        return new CosmosConfiguration(
            configuration.ConnectionString,
            configuration.DatabaseName,
            configuration.ContainerName,
            configuration.PartitionKey);
    }

    public static CosmosDocument ToDomainDocument(string id, string partitionKeyValue, string jsonPayload)
    {
        using var jsonDocument = JsonDocument.Parse(jsonPayload);
        return new CosmosDocument(id, partitionKeyValue, jsonDocument.RootElement);
    }

    public static IReadOnlyList<PatchOperation> ToPatchOperations(IEnumerable<PatchOperationDto> operations)
    {
        return operations.Select(MapPatchOperation).ToList();
    }

    public static DocumentResponse ToDocumentResponse(CosmosDocument document)
    {
        return new DocumentResponse
        {
            Id = document.Id,
            PartitionKeyValue = document.PartitionKeyValue,
            JsonPayload = JsonSerializer.Serialize(
                document.Payload,
                new JsonSerializerOptions { WriteIndented = true }),
            Timestamp = document.UpdatedAt ?? document.CreatedAt
        };
    }

    private static PatchOperation MapPatchOperation(PatchOperationDto operation)
    {
        if (!Enum.TryParse<PatchOperationType>(operation.OperationType, ignoreCase: true, out var operationType))
        {
            throw new ArgumentException(
                $"Invalid patch operation type '{operation.OperationType}'.",
                nameof(operation.OperationType));
        }

        return new PatchOperation(operationType, operation.Path, operation.Value);
    }
}
