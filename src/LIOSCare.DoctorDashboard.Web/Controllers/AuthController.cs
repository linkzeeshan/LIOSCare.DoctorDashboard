using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers;

[Route("auth")]
public sealed class AuthController(IAuthService authService) : Controller
{
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Dashboard");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null, CancellationToken ct = default)
    {
        var response = await authService.LoginAsync(new LoginRequest(email, password), ct);
        if (response is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid doctor email or password.");
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, response.Doctor.DoctorId.ToString()),
            new("doctor_id", response.Doctor.DoctorId.ToString()),
            new("account_id", response.Doctor.AccountId.ToString()),
            new(ClaimTypes.Email, response.Doctor.Email),
            new(ClaimTypes.Name, response.Doctor.FullName),
            new(ClaimTypes.Role, "doctor"),
            new("access_token", response.AccessToken),
            new("refresh_token", response.RefreshToken)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        });
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct = default)
    {
        var refresh = User.FindFirstValue("refresh_token");
        if (!string.IsNullOrWhiteSpace(refresh)) await authService.LogoutAsync(refresh, ct);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword() => View();

    [HttpPost("forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email, CancellationToken ct = default)
    {
        await authService.RequestPasswordResetAsync(email?.Trim() ?? string.Empty, ct);
        ViewBag.Sent = true;
        return View();
    }

    [HttpGet("reset-password")]
    public IActionResult ResetPassword(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return RedirectToAction(nameof(ForgotPassword));
        return View(model: token);
    }

    [HttpPost("reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword, CancellationToken ct = default)
    {
        if (newPassword != confirmPassword)
        {
            ViewData["Error"] = "Passwords do not match.";
            return View(model: token);
        }
        try
        {
            await authService.ResetPasswordAsync(token, newPassword, ct);
            return RedirectToAction(nameof(ResetPasswordSuccess));
        }
        catch (LIOSCare.DoctorDashboard.Application.Common.AppException ex)
        {
            ViewData["Error"] = ex.Message;
            return View(model: token);
        }
    }

    [HttpGet("reset-password/success")]
    public IActionResult ResetPasswordSuccess() => View();

    [HttpGet("access-denied")]
    public IActionResult AccessDenied() => View();
}
