using CosmosDbManager.Domain.Enums;

namespace CosmosDbManager.Application.Tests.Domain.Enums;

public sealed class PatchOperationTypeTests
{
    [Theory]
    [InlineData("Add", PatchOperationType.Add)]
    [InlineData("Set", PatchOperationType.Set)]
    [InlineData("Replace", PatchOperationType.Replace)]
    [InlineData("Remove", PatchOperationType.Remove)]
    [InlineData("Increment", PatchOperationType.Increment)]
    public void Enum_TryParse_ValidValue_ReturnsExpected(string text, PatchOperationType expected)
    {
        var parsed = Enum.TryParse<PatchOperationType>(text, ignoreCase: true, out var actual);

        Assert.True(parsed);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Enum_TryParse_InvalidValue_ReturnsFalse()
    {
        var parsed = Enum.TryParse<PatchOperationType>("UnknownOperation", ignoreCase: true, out _);

        Assert.False(parsed);
    }
}
