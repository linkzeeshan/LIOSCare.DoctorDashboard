namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class DoctorEducation
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string Degree { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int Year { get; set; }
    public DoctorProfile? Doctor { get; set; }
}
