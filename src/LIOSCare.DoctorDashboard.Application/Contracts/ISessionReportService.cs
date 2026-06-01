using LIOSCare.DoctorDashboard.Application.DTOs;

namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface ISessionReportService
{
    Task<(IReadOnlyList<SessionReportDto> Items, int Total)> GetReportsAsync(Guid doctorId, int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task<SessionReportDto> GetByIdAsync(Guid doctorId, Guid id, CancellationToken ct = default);
    Task<SessionReportDto> CreateAsync(Guid doctorId, CreateSessionReportRequest request, CancellationToken ct = default);
    Task<SessionReportDto> UpdateAsync(Guid doctorId, Guid id, UpdateSessionReportRequest request, CancellationToken ct = default);
}
