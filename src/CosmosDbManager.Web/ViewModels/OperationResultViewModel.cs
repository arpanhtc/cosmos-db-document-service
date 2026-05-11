using System.Text.Json;
using CosmosDbManager.Application.DTOs.Response;

namespace CosmosDbManager.Web.ViewModels;

public sealed class OperationResultViewModel
{
    public bool IsSuccess { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Message { get; set; }

    public string? DocumentId { get; set; }

    public string? PartitionKeyValue { get; set; }

    public string? JsonPayload { get; set; }

    public string? RawResponseJson { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public static OperationResultViewModel From(OperationResult<DocumentResponse> result)
    {
        var vm = new OperationResultViewModel
        {
            IsSuccess = result.IsSuccess,
            ErrorCode = result.ErrorCode,
            ErrorMessage = result.ErrorMessage,
            RawResponseJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })
        };

        if (result.IsSuccess && result.Data is not null)
        {
            vm.Message = "Operation completed successfully.";
            vm.DocumentId = result.Data.Id;
            vm.PartitionKeyValue = result.Data.PartitionKeyValue;
            vm.JsonPayload = result.Data.JsonPayload;
            vm.Timestamp = result.Data.Timestamp;
        }
        else
        {
            vm.Message = result.ErrorMessage;
        }

        return vm;
    }
}
