namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class DoctorAccount
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public DoctorProfile? DoctorProfile { get; set; }
    public List<DoctorRefreshToken> RefreshTokens { get; set; } = new();
    public List<PasswordResetToken> PasswordResetTokens { get; set; } = new();
}
