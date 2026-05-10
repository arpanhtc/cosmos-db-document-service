namespace CosmosDbManager.Domain.Exceptions;

public sealed class DocumentConflictException : CosmosDbException
{
    public DocumentConflictException(string id, string containerName)
        : base($"A document with ID '{id}' already exists in container '{containerName}'.")
    {
        DocumentId = id;
        ContainerName = containerName;
    }

    public string DocumentId { get; }

    public string ContainerName { get; }
}
