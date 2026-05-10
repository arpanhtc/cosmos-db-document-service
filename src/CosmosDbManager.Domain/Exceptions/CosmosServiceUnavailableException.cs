namespace CosmosDbManager.Domain.Exceptions;

public sealed class CosmosServiceUnavailableException : CosmosDbException
{
    public CosmosServiceUnavailableException(string message)
        : base(message)
    {
    }

    public CosmosServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
