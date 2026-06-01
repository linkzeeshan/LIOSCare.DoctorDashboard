using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers.Api;

[ApiController]
[Route("api/v1/auth")]
public sealed class ApiAuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var response = await auth.LoginAsync(request, ct);
        return response is null
            ? Unauthorized(ApiResponse.Err("Invalid doctor credentials.", 401))
            : Ok(ApiResponse.Data(response));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var response = await auth.RefreshAsync(request, ct);
        return response is null
            ? Unauthorized(ApiResponse.Err("Refresh token expired or invalid.", 401))
            : Ok(ApiResponse.Data(response));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct)
    {
        await auth.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordApiRequest request, CancellationToken ct)
    {
        await auth.RequestPasswordResetAsync(request.Email?.Trim() ?? string.Empty, ct);
        return Accepted(ApiResponse.Data(new { message = "If this email is associated with a doctor account, reset instructions will be sent." }));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordApiRequest request, CancellationToken ct)
    {
        try
        {
            await auth.ResetPasswordAsync(request.Token ?? string.Empty, request.NewPassword ?? string.Empty, ct);
            return Ok(ApiResponse.Data(new { message = "Password reset successfully. Please sign in with your new password." }));
        }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }
}

public sealed record ForgotPasswordApiRequest(string? Email);
public sealed record ResetPasswordApiRequest(string? Token, string? NewPassword);
