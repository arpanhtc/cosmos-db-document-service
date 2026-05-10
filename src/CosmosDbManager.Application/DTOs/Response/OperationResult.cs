namespace CosmosDbManager.Application.DTOs.Response;

public sealed class OperationResult<T>
{
    public bool IsSuccess { get; init; }

    public T? Data { get; init; }

    public string? ErrorMessage { get; init; }

    public string? ErrorCode { get; init; }

    public static OperationResult<T> Success(T data)
    {
        return new OperationResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static OperationResult<T> Failure(string errorMessage, string? errorCode = null)
    {
        return new OperationResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}
