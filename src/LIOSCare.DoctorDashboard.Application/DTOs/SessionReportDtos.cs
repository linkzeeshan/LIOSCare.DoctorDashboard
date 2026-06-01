namespace LIOSCare.DoctorDashboard.Application.DTOs;

public sealed record SessionReportDto(
    Guid Id,
    Guid BookingId,
    Guid DoctorId,
    Guid UserId,
    string UserFullName,
    int SessionNumber,
    DateTime SessionDate,
    int MoodBefore,
    int MoodAfter,
    int GoalsCompleted,
    int GoalsTotal,
    string ProgressNotes,
    string[] TechniquesUsed,
    string? NextSteps,
    DateTimeOffset CreatedAt,
    bool IsEditable);

public sealed record CreateSessionReportRequest(
    Guid BookingId,
    DateTime SessionDate,
    int MoodBefore,
    int MoodAfter,
    int GoalsCompleted,
    int GoalsTotal,
    string ProgressNotes,
    string[] TechniquesUsed,
    string? NextSteps);

public sealed record UpdateSessionReportRequest(
    DateTime SessionDate,
    int MoodBefore,
    int MoodAfter,
    int GoalsCompleted,
    int GoalsTotal,
    string ProgressNotes,
    string[] TechniquesUsed,
    string? NextSteps);
