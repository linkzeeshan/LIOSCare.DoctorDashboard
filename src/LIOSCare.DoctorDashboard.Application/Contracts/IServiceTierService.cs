using LIOSCare.DoctorDashboard.Application.DTOs;

namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface IServiceTierService
{
    Task<IReadOnlyList<ServiceTierDto>> GetActiveAsync(CancellationToken ct = default);
}
