namespace LIOSCare.DoctorDashboard.Web.Services;

public interface IPhotoStorageService
{
    Task<string> SaveAsync(IFormFile file, Guid doctorId, CancellationToken ct = default);
    Task TryDeleteAsync(string? publicUrl, CancellationToken ct = default);
}
