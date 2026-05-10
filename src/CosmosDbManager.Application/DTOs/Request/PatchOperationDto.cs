namespace CosmosDbManager.Application.DTOs.Request;

public sealed class PatchOperationDto
{
    public string OperationType { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public object? Value { get; set; }
}
