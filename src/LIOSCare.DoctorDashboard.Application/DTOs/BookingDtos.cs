namespace LIOSCare.DoctorDashboard.Application.DTOs;

public sealed record BookingDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    string? UserPhotoUrl,
    Guid DoctorId,
    Guid TierId,
    string TierName,
    decimal AmountUsd,
    decimal PlatformFeeUsd,
    string Status,
    string PaymentStatus,
    DateTimeOffset? ScheduledAt,
    string? Notes,
    DateTimeOffset CreatedAt,
    string? DeclineReason,
    bool RequiresSchedule);

public sealed record ScheduleBookingRequest(DateTimeOffset ScheduledAt);
public sealed record DeclineBookingRequest(string Reason, string? Note);
