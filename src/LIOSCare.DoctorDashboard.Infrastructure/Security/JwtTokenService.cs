using LIOSCare.DoctorDashboard.Application.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LIOSCare.DoctorDashboard.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public (string token, DateTimeOffset expiresAt) CreateAccessToken(DoctorUserDto doctor)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, doctor.AccountId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, doctor.DoctorId.ToString()),
            new Claim("doctor_id", doctor.DoctorId.ToString()),
            new Claim("account_id", doctor.AccountId.ToString()),
            new Claim(ClaimTypes.Email, doctor.Email),
            new Claim(ClaimTypes.Name, doctor.FullName),
            new Claim(ClaimTypes.Role, "doctor"),
            new Claim("role", "doctor")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public string CreateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
