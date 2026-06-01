namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class DoctorRefreshToken
{
    public Guid Id { get; set; }
    public Guid DoctorAccountId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DoctorAccount? DoctorAccount { get; set; }
}
