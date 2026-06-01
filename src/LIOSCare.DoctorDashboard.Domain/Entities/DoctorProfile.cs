namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class DoctorProfile
{
    public Guid Id { get; set; }
    public Guid DoctorAccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string[] Specializations { get; set; } = Array.Empty<string>();
    public int YearsExperience { get; set; }
    public decimal Rating { get; set; }
    public int PatientCount { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string[] Certifications { get; set; } = Array.Empty<string>();
    public string? ProfilePhotoUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public DoctorAccount? Account { get; set; }
    public List<DoctorEducation> Education { get; set; } = new();
    public List<QuickChatRequest> QuickChats { get; set; } = new();
    public List<DirectBookingRequest> Bookings { get; set; } = new();
}
