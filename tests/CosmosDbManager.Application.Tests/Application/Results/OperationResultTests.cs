using CosmosDbManager.Application.DTOs.Response;
using FluentAssertions;

namespace CosmosDbManager.Application.Tests.Application.Results;

public sealed class OperationResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessfulResultWithData()
    {
        var result = OperationResult<string>.Success("ok");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be("ok");
        result.ErrorMessage.Should().BeNull();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldReturnFailedResultWithErrorDetails()
    {
        var result = OperationResult<string>.Failure("failed", "ERR_CODE");

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("failed");
        result.ErrorCode.Should().Be("ERR_CODE");
    }
}
