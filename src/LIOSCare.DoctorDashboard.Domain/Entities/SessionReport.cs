namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class SessionReport
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int SessionNumber { get; set; }
    public DateTime SessionDate { get; set; }
    public int MoodBefore { get; set; }
    public int MoodAfter { get; set; }
    public int GoalsCompleted { get; set; }
    public int GoalsTotal { get; set; }
    public string ProgressNotes { get; set; } = string.Empty;
    public string[] TechniquesUsed { get; set; } = Array.Empty<string>();
    public string? NextSteps { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public DirectBookingRequest? Booking { get; set; }
    public DoctorProfile? Doctor { get; set; }

    public bool CanEdit(DateTimeOffset now) => CreatedAt >= now.AddHours(-24);
}
