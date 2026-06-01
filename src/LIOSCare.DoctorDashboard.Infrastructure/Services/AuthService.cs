using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Domain.Entities;
using LIOSCare.DoctorDashboard.Infrastructure.Mapping;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using LIOSCare.DoctorDashboard.Infrastructure.Security;
using LIOSCare.DoctorDashboard.Infrastructure.Services.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services;

public sealed class AuthService(
    DoctorPortalDbContext db,
    PasswordHashingService passwordHasher,
    JwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions,
    IOptions<PasswordResetOptions> resetOptions,
    IEmailService emailService) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly PasswordResetOptions _resetOptions = resetOptions.Value;

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var account = await db.DoctorAccounts
            .Include(x => x.DoctorProfile)
            .FirstOrDefaultAsync(x => x.Email == email && x.IsActive, ct);

        if (account?.DoctorProfile is null) return null;
        if (!passwordHasher.Verify(request.Password, account.PasswordHash)) return null;

        account.LastLoginAt = DateTimeOffset.UtcNow;
        return await IssueTokenAsync(account, ct);
    }

    public async Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var hash = passwordHasher.HashToken(request.RefreshToken);
        var refresh = await db.DoctorRefreshTokens
            .Include(x => x.DoctorAccount)!
            .ThenInclude(x => x!.DoctorProfile)
            .FirstOrDefaultAsync(x => x.TokenHash == hash && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow, ct);

        if (refresh?.DoctorAccount?.DoctorProfile is null) return null;
        refresh.RevokedAt = DateTimeOffset.UtcNow;
        refresh.RevokedReason = "Rotated";
        return await IssueTokenAsync(refresh.DoctorAccount, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = passwordHasher.HashToken(refreshToken);
        var token = await db.DoctorRefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (token is null) return;
        token.RevokedAt = DateTimeOffset.UtcNow;
        token.RevokedReason = "Logout";
        await db.SaveChangesAsync(ct);
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var normalised = (email ?? string.Empty).Trim().ToLowerInvariant();
        var account = await db.DoctorAccounts
            .FirstOrDefaultAsync(x => x.Email == normalised && x.IsActive, ct);

        if (account is null)
        {
            await Task.Delay(150, ct);
            return;
        }

        await db.PasswordResetTokens
            .Where(x => x.DoctorAccountId == account.Id && x.UsedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ExpiresAt, DateTimeOffset.UtcNow.AddMinutes(-1)), ct);

        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToBase64String(rawBytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        var tokenHash = passwordHasher.HashToken(rawToken);

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            DoctorAccountId = account.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_resetOptions.TokenExpiryMinutes),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);

        var resetUrl = $"{_resetOptions.ResetUrlBase.TrimEnd('/')}/auth/reset-password?token={Uri.EscapeDataString(rawToken)}";
        await emailService.SendAsync(
            account.Email,
            "LIOS Care — Reset your doctor dashboard password",
            BuildResetEmailHtml(resetUrl, _resetOptions.TokenExpiryMinutes),
            ct);
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        ValidatePasswordComplexity(newPassword);

        var hash = passwordHasher.HashToken(token ?? string.Empty);
        var resetToken = await db.PasswordResetTokens
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (resetToken is null || !resetToken.IsValid)
            throw new AppException("This reset link is invalid or has expired. Please request a new one.", 410);

        resetToken.Account!.PasswordHash = passwordHasher.Hash(newPassword);
        resetToken.UsedAt = DateTimeOffset.UtcNow;

        await db.DoctorRefreshTokens
            .Where(x => x.DoctorAccountId == resetToken.DoctorAccountId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow)
                .SetProperty(x => x.RevokedReason, "PasswordReset"), ct);

        await db.SaveChangesAsync(ct);
    }

    private static void ValidatePasswordComplexity(string password)
    {
        if ((password ?? string.Empty).Length < 8)
            throw new AppException("Password must be at least 8 characters.", 400);
        if (!password.Any(char.IsUpper))
            throw new AppException("Password must contain at least one uppercase letter.", 400);
        if (!password.Any(char.IsLower))
            throw new AppException("Password must contain at least one lowercase letter.", 400);
        if (!password.Any(char.IsDigit))
            throw new AppException("Password must contain at least one digit.", 400);
    }

    private static string BuildResetEmailHtml(string resetUrl, int expiryMinutes) =>
        $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:Arial,sans-serif;max-width:520px;margin:40px auto;padding:24px;color:#1a1a2e;background:#f8fafc;">
            <div style="background:#fff;border-radius:16px;padding:32px;box-shadow:0 2px 12px rgba(0,0,0,.07);">
                <h2 style="margin:0 0 4px;color:#2563eb;">LIOS Care</h2>
                <p style="margin:0 0 24px;color:#64748b;font-size:13px;">Doctor Dashboard</p>
                <h3 style="margin:0 0 12px;">Reset your password</h3>
                <p style="color:#475569;">A password reset was requested for your doctor account. Click the button below to choose a new password.</p>
                <div style="margin:28px 0;">
                    <a href="{resetUrl}" style="background:#2563eb;color:#fff;padding:13px 28px;border-radius:10px;text-decoration:none;font-weight:700;display:inline-block;">Reset my password</a>
                </div>
                <p style="color:#94a3b8;font-size:13px;">This link expires in <strong>{expiryMinutes} minutes</strong>. If you did not request a reset, you can safely ignore this email.</p>
                <hr style="border:none;border-top:1px solid #e2e8f0;margin:24px 0;"/>
                <p style="color:#cbd5e1;font-size:11px;">LIOS Care · Doctor Dashboard · Automated message — do not reply.</p>
            </div>
        </body>
        </html>
        """;

    private async Task<AuthResponse> IssueTokenAsync(DoctorAccount account, CancellationToken ct)
    {
        var doctor = account.DoctorProfile!.ToDoctorUser(account.Email);
        var access = jwtTokenService.CreateAccessToken(doctor);
        var refreshToken = jwtTokenService.CreateRefreshToken();

        db.DoctorRefreshTokens.Add(new DoctorRefreshToken
        {
            Id = Guid.NewGuid(),
            DoctorAccountId = account.Id,
            TokenHash = passwordHasher.HashToken(refreshToken),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        });
        await db.SaveChangesAsync(ct);
        return new AuthResponse(access.token, refreshToken, access.expiresAt, doctor);
    }
}
