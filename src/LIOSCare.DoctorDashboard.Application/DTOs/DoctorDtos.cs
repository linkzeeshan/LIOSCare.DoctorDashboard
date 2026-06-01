namespace LIOSCare.DoctorDashboard.Application.DTOs;

public sealed record EducationDto(Guid? Id, string Degree, string Institution, int Year);

public sealed record DoctorProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string[] Specializations,
    int YearsExperience,
    decimal Rating,
    int PatientCount,
    string Bio,
    string[] Certifications,
    IReadOnlyList<EducationDto> Education,
    string? ProfilePhotoUrl,
    bool IsAvailable);

public sealed record UpdateDoctorProfileRequest(
    string FullName,
    string[] Specializations,
    int YearsExperience,
    string Bio,
    string[] Certifications,
    List<EducationDto> Education,
    bool IsAvailable);

public sealed record DoctorStatsDto(int TotalSessions, decimal AverageMoodImprovement, int PendingQuickChats, int PendingBookings);
