using CosmosDbManager.Application.DTOs.Request;
using FluentValidation;

namespace CosmosDbManager.Application.Validators;

public sealed class GetDocumentValidator : AbstractValidator<GetDocumentRequest>
{
    public GetDocumentValidator()
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
    }
}
