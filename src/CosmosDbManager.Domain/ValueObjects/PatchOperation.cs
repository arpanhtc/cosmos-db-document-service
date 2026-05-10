using CosmosDbManager.Domain.Enums;

namespace CosmosDbManager.Domain.ValueObjects;

public sealed record PatchOperation
{
    public PatchOperation(PatchOperationType operationType, string path, object? value = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
        }

        if (!path.StartsWith('/'))
        {
            throw new ArgumentException("Path must start with '/'.", nameof(path));
        }

        OperationType = operationType;
        Path = path.Trim();
        Value = value;
    }

    public PatchOperationType OperationType { get; }

    public string Path { get; }

    public object? Value { get; }
}
