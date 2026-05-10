using CosmosDbManager.Application.DTOs.Request;
using CosmosDbManager.Domain.Enums;
using FluentValidation;
using System.Globalization;
using System.Text.Json;

namespace CosmosDbManager.Application.Validators;

public sealed class PatchOperationDtoValidator : AbstractValidator<PatchOperationDto>
{
    public PatchOperationDtoValidator()
    {
        RuleFor(x => x.OperationType)
            .NotEmpty()
            .WithMessage("OperationType is required.")
            .Must(BeValidOperationType)
            .WithMessage("OperationType must be one of: Add, Set, Replace, Remove, Increment.");

        RuleFor(x => x.Path)
            .NotEmpty()
            .WithMessage("Path is required.")
            .Must(path => path.StartsWith('/'))
            .WithMessage("Path must start with '/'.");

        RuleFor(x => x.Value)
            .NotNull()
            .When(x => RequiresValue(x.OperationType))
            .WithMessage("Value is required for this operation.");

        RuleFor(x => x.Value)
            .Must(BeNumeric)
            .When(x => IsIncrement(x.OperationType))
            .WithMessage("Value must be numeric for Increment operation.");
    }

    private static bool BeValidOperationType(string operationType)
    {
        return Enum.TryParse<PatchOperationType>(operationType, ignoreCase: true, out _);
    }

    private static bool RequiresValue(string operationType)
    {
        return !IsRemove(operationType);
    }

    private static bool IsIncrement(string operationType)
    {
        return operationType.Equals(PatchOperationType.Increment.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRemove(string operationType)
    {
        return operationType.Equals(PatchOperationType.Remove.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool BeNumeric(object? value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number => true,
                JsonValueKind.String => double.TryParse(
                    jsonElement.GetString(),
                    NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture,
                    out _),
                _ => false
            };
        }

        if (value is string stringValue)
        {
            return double.TryParse(
                stringValue,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out _);
        }

        return value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }
}
