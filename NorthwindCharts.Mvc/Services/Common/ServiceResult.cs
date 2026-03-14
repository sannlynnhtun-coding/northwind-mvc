namespace NorthwindCharts.Mvc.Services.Common;

public sealed class ServiceResult
{
    private ServiceResult(bool success, string? error)
    {
        Success = success;
        Error = error;
    }

    public bool Success { get; }

    public string? Error { get; }

    public static ServiceResult Ok() => new(true, null);

    public static ServiceResult Fail(string error) => new(false, error);
}
