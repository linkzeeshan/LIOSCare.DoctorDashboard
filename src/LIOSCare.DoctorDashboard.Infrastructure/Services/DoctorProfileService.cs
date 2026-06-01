using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Domain.Entities;
using LIOSCare.DoctorDashboard.Infrastructure.Mapping;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services;

public sealed class DoctorProfileService(DoctorPortalDbContext db) : IDoctorProfileService
{
    public async Task<DoctorProfileDto> GetMeAsync(Guid doctorId, CancellationToken ct = default)
    {
        var profile = await db.DoctorProfiles.Include(x => x.Account).Include(x => x.Education)
            .FirstOrDefaultAsync(x => x.Id == doctorId, ct)
            ?? throw new AppException("Doctor profile not found.", 404);
        return profile.ToDto(profile.Account?.Email ?? string.Empty);
    }

    public async Task<DoctorProfileDto> UpdateMeAsync(Guid doctorId, UpdateDoctorProfileRequest request, CancellationToken ct = default)
    {
        var profile = await db.DoctorProfiles.Include(x => x.Account).Include(x => x.Education)
            .FirstOrDefaultAsync(x => x.Id == doctorId, ct)
            ?? throw new AppException("Doctor profile not found.", 404);

        profile.FullName = request.FullName.Trim();
        profile.Specializations = request.Specializations.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToArray();
        profile.YearsExperience = request.YearsExperience;
        profile.Bio = request.Bio.Trim();
        profile.Certifications = request.Certifications.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToArray();
        profile.IsAvailable = request.IsAvailable;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        db.DoctorEducations.RemoveRange(profile.Education);
        profile.Education = request.Education.Select(e => new DoctorEducation
        {
            Id = Guid.NewGuid(), DoctorId = doctorId, Degree = e.Degree.Trim(), Institution = e.Institution.Trim(), Year = e.Year
        }).ToList();

        await db.SaveChangesAsync(ct);
        return profile.ToDto(profile.Account?.Email ?? string.Empty);
    }

    public async Task SetAvailabilityAsync(Guid doctorId, bool isAvailable, CancellationToken ct = default)
    {
        var updated = await db.DoctorProfiles.Where(x => x.Id == doctorId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsAvailable, isAvailable).SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), ct);
        if (updated == 0) throw new AppException("Doctor profile not found.", 404);
    }

    public async Task SetProfilePhotoAsync(Guid doctorId, string photoUrl, CancellationToken ct = default)
    {
        var updated = await db.DoctorProfiles.Where(x => x.Id == doctorId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProfilePhotoUrl, photoUrl).SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), ct);
        if (updated == 0) throw new AppException("Doctor profile not found.", 404);
    }

    public async Task<DoctorStatsDto> GetStatsAsync(Guid doctorId, CancellationToken ct = default)
    {
        var totalSessions = await db.SessionReports.CountAsync(x => x.DoctorId == doctorId, ct);
        var mood = totalSessions == 0 ? 0 : await db.SessionReports.Where(x => x.DoctorId == doctorId).AverageAsync(x => (decimal)(x.MoodAfter - x.MoodBefore), ct);
        var pendingQuickChats = await db.QuickChatRequests.CountAsync(x => (x.DoctorId == doctorId || x.DoctorId == null) && x.Status == Domain.Enums.QuickChatStatus.Pending, ct);
        var pendingBookings = await db.DirectBookingRequests.CountAsync(x => x.DoctorId == doctorId && x.Status == Domain.Enums.BookingStatus.Pending, ct);
        return new DoctorStatsDto(totalSessions, Math.Round(mood, 1), pendingQuickChats, pendingBookings);
    }
}
