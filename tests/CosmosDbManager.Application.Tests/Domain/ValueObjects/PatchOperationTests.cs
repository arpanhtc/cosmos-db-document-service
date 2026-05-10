using CosmosDbManager.Domain.Enums;
using CosmosDbManager.Domain.ValueObjects;

namespace CosmosDbManager.Application.Tests.Domain.ValueObjects;

public sealed class PatchOperationTests
{
    [Fact]
    public void Constructor_ValidValues_InitializesOperation()
    {
        var operation = new PatchOperation(PatchOperationType.Set, "/status", "active");

        Assert.Equal(PatchOperationType.Set, operation.OperationType);
        Assert.Equal("/status", operation.Path);
        Assert.Equal("active", operation.Value);
    }

    [Fact]
    public void Constructor_PathWithoutSlash_ThrowsArgumentException()
    {
        var action = () => new PatchOperation(PatchOperationType.Add, "status", "active");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("path", exception.ParamName);
    }
}
