namespace CosmosDbManager.Web.ViewModels;

public abstract class DocumentFormViewModel
{
    public ConfigurationViewModel Configuration { get; set; } = new();

    public string Id { get; set; } = string.Empty;

    public string PartitionKeyValue { get; set; } = string.Empty;

    public OperationResultViewModel? Result { get; set; }
}
