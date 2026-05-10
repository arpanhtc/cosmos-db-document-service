using System.Text.Json;
using CosmosDbManager.Application.DTOs.Request;
using FluentValidation;

namespace CosmosDbManager.Application.Validators;

public sealed class UpsertDocumentValidator : AbstractValidator<UpsertDocumentRequest>
{
    public UpsertDocumentValidator()
    {
        RuleFor(x => x.Configuration)
            .NotNull()
            .WithMessage("Configuration is required.");

        When(x => x.Configuration is not null, () =>
        {
            RuleFor(x => x.Configuration!)
                .SetValidator(new CosmosConfigurationValidator());
        });

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required.")
            .MaximumLength(255)
            .WithMessage("Id must be 255 characters or fewer.");

        RuleFor(x => x.PartitionKeyValue)
            .NotEmpty()
            .WithMessage("PartitionKeyValue is required.");

        RuleFor(x => x.JsonPayload)
            .NotEmpty()
            .WithMessage("JsonPayload is required.")
            .Must(BeValidJson)
            .WithMessage("JsonPayload must be valid JSON.");
    }

    private static bool BeValidJson(string payload)
    {
        try
        {
            using var _ = JsonDocument.Parse(payload);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
