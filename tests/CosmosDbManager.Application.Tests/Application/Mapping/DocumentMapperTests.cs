using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.Mappings;
using CosmosDbManager.Domain.Entities;
using CosmosDbManager.Domain.Enums;
using FluentAssertions;

namespace CosmosDbManager.Application.Tests.Application.Mapping;

public sealed class DocumentMapperTests
{
    [Fact]
    public void ToCosmosConfiguration_ShouldMapDtoValues()
    {
        var dto = new CosmosConfigurationDto
        {
            ConnectionString = "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;",
            DatabaseName = "db1",
            ContainerName = "container1",
            PartitionKey = "/tenantId"
        };

        var result = DocumentMapper.ToCosmosConfiguration(dto);

        result.ConnectionString.Should().Be(dto.ConnectionString);
        result.DatabaseName.Should().Be(dto.DatabaseName);
        result.ContainerName.Should().Be(dto.ContainerName);
        result.PartitionKey.Should().Be(dto.PartitionKey);
    }

    [Fact]
    public void ToDomainDocument_ShouldPreserveJsonPayload()
    {
        var result = DocumentMapper.ToDomainDocument(
            "doc-1",
            "tenant-1",
            """{"id":"doc-1","tenantId":"tenant-1","status":"active"}""");

        result.Id.Should().Be("doc-1");
        result.PartitionKeyValue.Should().Be("tenant-1");
        result.Payload.GetProperty("status").GetString().Should().Be("active");
    }

    [Fact]
    public void ToPatchOperations_ShouldMapStringOperationTypes()
    {
        var operations = new List<PatchOperationDto>
        {
            new() { OperationType = "Set", Path = "/status", Value = "active" },
            new() { OperationType = "Increment", Path = "/count", Value = 1 }
        };

        var result = DocumentMapper.ToPatchOperations(operations);

        result.Should().HaveCount(2);
        result[0].OperationType.Should().Be(PatchOperationType.Set);
        result[1].OperationType.Should().Be(PatchOperationType.Increment);
    }

    [Fact]
    public void ToDocumentResponse_ShouldSerializeIndentedJsonAndTimestamp()
    {
        var document = DocumentMapper.ToDomainDocument(
            "doc-1",
            "tenant-1",
            """{"a":1,"b":"x"}""");

        var updated = new CosmosDocument(
            document.Id,
            document.PartitionKeyValue,
            document.Payload,
            createdAt: DateTimeOffset.UtcNow.AddMinutes(-10),
            updatedAt: DateTimeOffset.UtcNow);

        var response = DocumentMapper.ToDocumentResponse(updated);

        response.Id.Should().Be("doc-1");
        response.PartitionKeyValue.Should().Be("tenant-1");
        response.Timestamp.Should().Be(updated.UpdatedAt);
        response.JsonPayload.Should().Contain("\n");
        response.JsonPayload.Should().Contain("\"a\": 1");
    }
}
