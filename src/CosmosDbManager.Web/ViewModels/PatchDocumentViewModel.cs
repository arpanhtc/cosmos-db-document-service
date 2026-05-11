using System.ComponentModel.DataAnnotations;

namespace CosmosDbManager.Web.ViewModels;

public sealed class PatchDocumentViewModel : DocumentFormViewModel
{
    [MinLength(1, ErrorMessage = "At least one patch operation is required.")]
    public List<PatchOperationViewModel> Operations { get; set; } = [new()];
}
