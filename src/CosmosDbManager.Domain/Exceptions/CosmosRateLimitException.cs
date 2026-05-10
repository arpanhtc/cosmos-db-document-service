namespace CosmosDbManager.Domain.Exceptions;

public sealed class CosmosRateLimitException : CosmosDbException
{
    public CosmosRateLimitException(string message, TimeSpan? retryAfter = null)
        : base(message)
    {
        RetryAfter = retryAfter;
    }

    public CosmosRateLimitException(string message, Exception innerException, TimeSpan? retryAfter = null)
        : base(message, innerException)
    {
        RetryAfter = retryAfter;
    }

    public TimeSpan? RetryAfter { get; }
}
