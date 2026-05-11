using System.ComponentModel.DataAnnotations;

namespace CosmosDbManager.Web.ViewModels;

public class InsertDocumentViewModel : DocumentFormViewModel
{
    [Required]
    [Display(Name = "JSON Payload")]
    [DataType(DataType.MultilineText)]
    public string JsonPayload { get; set; } =
        "{\n" +
        "  \"id\": \"doc-1\",\n" +
        "  \"tenantId\": \"tenant-1\",\n" +
        "  \"status\": \"active\"\n" +
        "}";
}
