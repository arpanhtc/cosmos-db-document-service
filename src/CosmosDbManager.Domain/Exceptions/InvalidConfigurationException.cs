namespace CosmosDbManager.Domain.Exceptions;

public sealed class InvalidConfigurationException : CosmosDbException
{
    public InvalidConfigurationException(string message)
        : base(message)
    {
    }

    public InvalidConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
