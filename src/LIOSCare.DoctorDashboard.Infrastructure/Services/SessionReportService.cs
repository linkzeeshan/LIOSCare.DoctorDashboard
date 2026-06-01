using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Domain.Entities;
using LIOSCare.DoctorDashboard.Domain.Enums;
using LIOSCare.DoctorDashboard.Infrastructure.Mapping;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services;

public sealed class SessionReportService(DoctorPortalDbContext db) : ISessionReportService
{
    public async Task<(IReadOnlyList<SessionReportDto> Items, int Total)> GetReportsAsync(Guid doctorId, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.SessionReports.AsNoTracking().Where(x => x.DoctorId == doctorId);
        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(x => x.SessionDate)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        return (rows.Select(x => x.ToDto(now)).ToList(), total);
    }

    public async Task<SessionReportDto> GetByIdAsync(Guid doctorId, Guid id, CancellationToken ct = default)
    {
        var report = await db.SessionReports.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.DoctorId == doctorId, ct)
            ?? throw new AppException("Session report not found.", 404);
        return report.ToDto(DateTimeOffset.UtcNow);
    }

    public async Task<SessionReportDto> CreateAsync(Guid doctorId, CreateSessionReportRequest request, CancellationToken ct = default)
    {
        var booking = await db.DirectBookingRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.BookingId && x.DoctorId == doctorId
                && (x.Status == BookingStatus.Accepted || x.Status == BookingStatus.Completed), ct)
            ?? throw new AppException("Select an accepted or completed booking to link this report.", 400);

        ValidateReport(request.MoodBefore, request.MoodAfter, request.GoalsCompleted, request.GoalsTotal, request.ProgressNotes, request.NextSteps);
        var nextNumber = await db.SessionReports.CountAsync(x => x.DoctorId == doctorId && x.UserId == booking.UserId, ct) + 1;
        var now = DateTimeOffset.UtcNow;
        var report = new SessionReport
        {
            Id = Guid.NewGuid(), BookingId = booking.Id, DoctorId = doctorId, UserId = booking.UserId,
            UserFullName = booking.UserFullName, SessionNumber = nextNumber, SessionDate = request.SessionDate.Date,
            MoodBefore = request.MoodBefore, MoodAfter = request.MoodAfter,
            GoalsCompleted = request.GoalsCompleted, GoalsTotal = request.GoalsTotal,
            ProgressNotes = request.ProgressNotes.Trim(), TechniquesUsed = request.TechniquesUsed.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToArray(),
            NextSteps = request.NextSteps?.Trim(), CreatedAt = now, UpdatedAt = now
        };
        db.SessionReports.Add(report);
        await db.SaveChangesAsync(ct);
        return report.ToDto(now);
    }

    public async Task<SessionReportDto> UpdateAsync(Guid doctorId, Guid id, UpdateSessionReportRequest request, CancellationToken ct = default)
    {
        var report = await db.SessionReports.FirstOrDefaultAsync(x => x.Id == id && x.DoctorId == doctorId, ct)
            ?? throw new AppException("Session report not found.", 404);
        var now = DateTimeOffset.UtcNow;
        if (!report.CanEdit(now)) throw new AppException("Reports older than 24 hours are read-only.", 403);
        ValidateReport(request.MoodBefore, request.MoodAfter, request.GoalsCompleted, request.GoalsTotal, request.ProgressNotes, request.NextSteps);
        report.SessionDate = request.SessionDate.Date;
        report.MoodBefore = request.MoodBefore;
        report.MoodAfter = request.MoodAfter;
        report.GoalsCompleted = request.GoalsCompleted;
        report.GoalsTotal = request.GoalsTotal;
        report.ProgressNotes = request.ProgressNotes.Trim();
        report.TechniquesUsed = request.TechniquesUsed.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToArray();
        report.NextSteps = request.NextSteps?.Trim();
        report.UpdatedAt = now;
        await db.SaveChangesAsync(ct);
        return report.ToDto(now);
    }

    private static void ValidateReport(int moodBefore, int moodAfter, int goalsCompleted, int goalsTotal, string notes, string? nextSteps = null)
    {
        if (moodBefore is < 1 or > 10 || moodAfter is < 1 or > 10) throw new AppException("Mood values must be between 1 and 10.");
        if (goalsCompleted < 0 || goalsTotal <= 0 || goalsCompleted > goalsTotal) throw new AppException("Goals completed cannot exceed goals total, and total must be at least 1.");
        if (string.IsNullOrWhiteSpace(notes)) throw new AppException("Progress notes are required.");
        if (notes.Length > 2000) throw new AppException("Progress notes must not exceed 2000 characters.");
        if (nextSteps is { Length: > 500 }) throw new AppException("Next steps must not exceed 500 characters.");
    }
}
