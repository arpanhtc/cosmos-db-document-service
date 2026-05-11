using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CosmosDbManager.Web.ViewModels;

public sealed class PatchOperationViewModel : IValidatableObject
{
    [Required]
    [Display(Name = "Operation Type")]
    public string OperationType { get; set; } = "Set";

    [Required]
    public string Path { get; set; } = string.Empty;

    [Display(Name = "Value")]
    public string? Value { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Path.StartsWith('/'))
        {
            yield return new ValidationResult("Path must start with '/'.", [nameof(Path)]);
        }

        var requiresValue = !OperationType.Equals("Remove", StringComparison.OrdinalIgnoreCase);
        var isIncrement = OperationType.Equals("Increment", StringComparison.OrdinalIgnoreCase);

        if (requiresValue && string.IsNullOrWhiteSpace(Value))
        {
            yield return new ValidationResult("Value is required for this operation.", [nameof(Value)]);
        }

        if (isIncrement && !string.IsNullOrWhiteSpace(Value)
            && !double.TryParse(Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _))
        {
            yield return new ValidationResult("Value must be numeric for Increment.", [nameof(Value)]);
        }
    }
}
