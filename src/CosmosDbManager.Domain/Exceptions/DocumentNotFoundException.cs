namespace CosmosDbManager.Domain.Exceptions;

public sealed class DocumentNotFoundException : CosmosDbException
{
    public DocumentNotFoundException(string id, string containerName)
        : base($"Document with ID '{id}' was not found in container '{containerName}'.")
    {
        DocumentId = id;
        ContainerName = containerName;
    }

    public string DocumentId { get; }

    public string ContainerName { get; }
}
