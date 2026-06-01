namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid DoctorAccountId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public DoctorAccount? Account { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
    public bool IsUsed => UsedAt.HasValue;
    public bool IsValid => !IsExpired && !IsUsed;
}
