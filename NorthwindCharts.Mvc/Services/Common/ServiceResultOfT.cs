namespace NorthwindCharts.Mvc.Services.Common;

public sealed class ServiceResult<T>
{
    private ServiceResult(bool success, T? value, string? error)
    {
        Success = success;
        Value = value;
        Error = error;
    }

    public bool Success { get; }

    public T? Value { get; }

    public string? Error { get; }

    public static ServiceResult<T> Ok(T value) => new(true, value, null);

    public static ServiceResult<T> Fail(string error) => new(false, default, error);
}
