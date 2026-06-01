using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Infrastructure.Mapping;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services;

public sealed class ServiceTierService(DoctorPortalDbContext db) : IServiceTierService
{
    public async Task<IReadOnlyList<ServiceTierDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var tiers = await db.ServiceTiers.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.PriceUsd)
            .ToListAsync(ct);
        return tiers.Select(x => x.ToDto()).ToList();
    }
}
