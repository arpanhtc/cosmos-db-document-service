using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Application.Validators;
using FluentAssertions;

namespace CosmosDbManager.Application.Tests.Application.Validators;

public sealed class PatchDocumentValidatorTests
{
    private readonly PatchDocumentValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyOperations_ShouldFail()
    {
        var request = new PatchDocumentRequest
        {
            Configuration = new CosmosConfigurationDto
            {
                ConnectionString = "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;",
                DatabaseName = "db-1",
                ContainerName = "users-1",
                PartitionKey = "/tenantId"
            },
            Id = "doc-1",
            PartitionKeyValue = "tenant-1",
            Operations = []
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.ErrorMessage).Should().Contain("At least one patch operation is required.");
    }

    [Fact]
    public void Validate_WithIncrementAndNonNumericValue_ShouldFail()
    {
        var request = new PatchDocumentRequest
        {
            Configuration = new CosmosConfigurationDto
            {
                ConnectionString = "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=abc;",
                DatabaseName = "db-1",
                ContainerName = "users-1",
                PartitionKey = "/tenantId"
            },
            Id = "doc-1",
            PartitionKeyValue = "tenant-1",
            Operations =
            [
                new PatchOperationDto
                {
                    OperationType = "Increment",
                    Path = "/count",
                    Value = "not-a-number"
                }
            ]
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.ErrorMessage).Should().Contain("Value must be numeric for Increment operation.");
    }
}
