using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NorthwindCharts.Mvc.Configuration;
using NorthwindCharts.Mvc.Infrastructure;
using NorthwindCharts.Mvc.ViewModels.Auth;

namespace NorthwindCharts.Mvc.Controllers;

[AllowAnonymous]
public sealed class AuthController : Controller
{
    private readonly AdminAuthOptions _authOptions;

    public AuthController(IOptions<AdminAuthOptions> authOptions)
    {
        _authOptions = authOptions.Value;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.Username.Trim();
        var password = model.Password;

        if (!string.Equals(username, _authOptions.Username, StringComparison.Ordinal) ||
            !string.Equals(password, _authOptions.Password, StringComparison.Ordinal))
        {
            model.ErrorMessage = "Invalid username or password.";
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, AuthConstants.CookieScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            AuthConstants.CookieScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return RedirectToLocal(returnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AuthConstants.CookieScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }
}
