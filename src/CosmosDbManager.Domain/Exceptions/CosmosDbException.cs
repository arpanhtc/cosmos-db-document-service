namespace CosmosDbManager.Domain.Exceptions;

public class CosmosDbException : Exception
{
    public CosmosDbException(string message)
        : base(message)
    {
    }

    public CosmosDbException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
