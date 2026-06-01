namespace LIOSCare.DoctorDashboard.Application.DTOs;

public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, DoctorUserDto Doctor);
public sealed record DoctorUserDto(Guid DoctorId, Guid AccountId, string Email, string FullName, string[] Specializations);
public sealed record RefreshTokenRequest(string RefreshToken);
