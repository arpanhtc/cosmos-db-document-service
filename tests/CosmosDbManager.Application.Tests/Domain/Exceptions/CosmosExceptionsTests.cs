using CosmosDbManager.Domain.Exceptions;

namespace CosmosDbManager.Application.Tests.Domain.Exceptions;

public sealed class CosmosExceptionsTests
{
    [Fact]
    public void DocumentNotFoundException_Constructor_SetsContextAndMessage()
    {
        var exception = new DocumentNotFoundException("doc-1", "users");

        Assert.Equal("doc-1", exception.DocumentId);
        Assert.Equal("users", exception.ContainerName);
        Assert.Contains("doc-1", exception.Message);
        Assert.Contains("users", exception.Message);
    }

    [Fact]
    public void DocumentConflictException_Constructor_SetsContextAndMessage()
    {
        var exception = new DocumentConflictException("doc-1", "users");

        Assert.Equal("doc-1", exception.DocumentId);
        Assert.Equal("users", exception.ContainerName);
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public void InvalidConfigurationException_WithInnerException_PreservesInnerException()
    {
        var innerException = new InvalidOperationException("Bad config");
        var exception = new InvalidConfigurationException("Invalid Cosmos configuration.", innerException);

        Assert.Equal("Invalid Cosmos configuration.", exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void CosmosRateLimitException_WithRetryAfter_SetsRetryAfter()
    {
        var retryAfter = TimeSpan.FromSeconds(5);
        var exception = new CosmosRateLimitException("Rate limited.", retryAfter);

        Assert.Equal(retryAfter, exception.RetryAfter);
    }

    [Fact]
    public void CosmosServiceUnavailableException_Constructor_SetsMessage()
    {
        var exception = new CosmosServiceUnavailableException("Service unavailable.");

        Assert.Equal("Service unavailable.", exception.Message);
    }
}
