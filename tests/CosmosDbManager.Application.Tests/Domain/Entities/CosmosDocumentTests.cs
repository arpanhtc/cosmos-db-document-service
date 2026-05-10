using System.Text.Json;
using CosmosDbManager.Domain.Entities;

namespace CosmosDbManager.Application.Tests.Domain.Entities;

public sealed class CosmosDocumentTests
{
    [Fact]
    public void Constructor_ValidValues_InitializesDocument()
    {
        using var json = JsonDocument.Parse("""{"name":"Alice","count":2}""");
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = createdAt.AddMinutes(5);

        var document = new CosmosDocument(
            id: "doc-1",
            partitionKeyValue: "tenant-1",
            payload: json.RootElement,
            createdAt: createdAt,
            updatedAt: updatedAt);

        Assert.Equal("doc-1", document.Id);
        Assert.Equal("tenant-1", document.PartitionKeyValue);
        Assert.Equal("Alice", document.Payload.GetProperty("name").GetString());
        Assert.Equal(createdAt, document.CreatedAt);
        Assert.Equal(updatedAt, document.UpdatedAt);
    }

    [Fact]
    public void Constructor_BlankId_ThrowsArgumentException()
    {
        using var json = JsonDocument.Parse("""{"name":"Alice"}""");

        var action = () => new CosmosDocument(" ", "tenant-1", json.RootElement);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("id", exception.ParamName);
    }
}
