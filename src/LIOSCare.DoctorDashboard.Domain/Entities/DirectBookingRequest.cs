using LIOSCare.DoctorDashboard.Domain.Enums;

namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class DirectBookingRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserPhotoUrl { get; set; }
    public Guid DoctorId { get; set; }
    public Guid TierId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public decimal AmountUsd { get; set; }
    public decimal PlatformFeeUsd { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public string? Notes { get; set; }
    public DeclineReason DeclineReason { get; set; } = DeclineReason.None;
    public string? DeclineNote { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? DeclinedAt { get; set; }

    public DoctorProfile? Doctor { get; set; }
    public ServiceTier? Tier { get; set; }
    public List<SessionReport> Reports { get; set; } = new();
}
