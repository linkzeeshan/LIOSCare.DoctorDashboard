using LIOSCare.DoctorDashboard.Application.DTOs;

namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface IDoctorProfileService
{
    Task<DoctorProfileDto> GetMeAsync(Guid doctorId, CancellationToken ct = default);
    Task<DoctorProfileDto> UpdateMeAsync(Guid doctorId, UpdateDoctorProfileRequest request, CancellationToken ct = default);
    Task SetAvailabilityAsync(Guid doctorId, bool isAvailable, CancellationToken ct = default);
    Task SetProfilePhotoAsync(Guid doctorId, string photoUrl, CancellationToken ct = default);
    Task<DoctorStatsDto> GetStatsAsync(Guid doctorId, CancellationToken ct = default);
}
