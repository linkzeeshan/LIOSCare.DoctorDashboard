using LIOSCare.DoctorDashboard.Application.DTOs;

namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface IDashboardQueryService
{
    Task<DashboardSummaryDto> GetDashboardAsync(Guid doctorId, CancellationToken ct = default);
}
