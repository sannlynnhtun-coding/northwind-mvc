using System.ComponentModel.DataAnnotations;

namespace NorthwindCharts.Mvc.ViewModels.Auth;

public sealed class LoginVm
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}
