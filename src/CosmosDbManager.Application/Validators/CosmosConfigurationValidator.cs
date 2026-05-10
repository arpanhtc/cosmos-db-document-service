using CosmosDbManager.Application.DTOs.Request;
using FluentValidation;

namespace CosmosDbManager.Application.Validators;

public sealed class CosmosConfigurationValidator : AbstractValidator<CosmosConfigurationDto>
{
    private const string NamePattern = "^[A-Za-z0-9-]+$";

    public CosmosConfigurationValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty()
            .WithMessage("ConnectionString is required.");

        RuleFor(x => x.DatabaseName)
            .NotEmpty()
            .WithMessage("DatabaseName is required.")
            .Matches(NamePattern)
            .WithMessage("DatabaseName may contain only letters, numbers, and hyphens.");

        RuleFor(x => x.ContainerName)
            .NotEmpty()
            .WithMessage("ContainerName is required.")
            .Matches(NamePattern)
            .WithMessage("ContainerName may contain only letters, numbers, and hyphens.");

        RuleFor(x => x.PartitionKey)
            .NotEmpty()
            .WithMessage("PartitionKey is required.")
            .Must(path => path.StartsWith('/'))
            .WithMessage("PartitionKey must start with '/'.");
    }
}
