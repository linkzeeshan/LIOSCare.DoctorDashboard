using Dapper;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Infrastructure.Mapping;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services;

public sealed class DashboardQueryService(DoctorPortalDbContext db, IConfiguration configuration) : IDashboardQueryService
{
    public async Task<DashboardSummaryDto> GetDashboardAsync(Guid doctorId, CancellationToken ct = default)
    {
        var cs = configuration.GetConnectionString("SocialPlatformDb")!;
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync(ct);

        var pendingChats = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            """select count(*) from provider.quick_chat_requests where status = 'Pending' and (doctor_id is null or doctor_id = @doctorId)""",
            new { doctorId }, cancellationToken: ct));
        var pendingBookings = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            """select count(*) from provider.direct_booking_requests where status = 'Pending' and doctor_id = @doctorId""",
            new { doctorId }, cancellationToken: ct));
        var sessionsThisMonth = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            """select count(*) from provider.doctor_session_reports where doctor_id = @doctorId and session_date >= date_trunc('month', now())""",
            new { doctorId }, cancellationToken: ct));
        var avgMood = await conn.ExecuteScalarAsync<decimal?>(new CommandDefinition(
            """select avg(mood_after - mood_before) from provider.doctor_session_reports where doctor_id = @doctorId""",
            new { doctorId }, cancellationToken: ct)) ?? 0;

        var profile = await db.DoctorProfiles.AsNoTracking().FirstAsync(x => x.Id == doctorId, ct);
        var recentChats = await db.QuickChatRequests.Include(x => x.Tier).AsNoTracking()
            .Where(x => x.Status == Domain.Enums.QuickChatStatus.Pending && (x.DoctorId == null || x.DoctorId == doctorId))
            .OrderBy(x => x.SlaDeadline).Take(5).ToListAsync(ct);
        var recentBookings = await db.DirectBookingRequests.Include(x => x.Tier).AsNoTracking()
            .Where(x => x.DoctorId == doctorId).OrderByDescending(x => x.CreatedAt).Take(3).ToListAsync(ct);
        var latestReports = await db.SessionReports.AsNoTracking()
            .Where(x => x.DoctorId == doctorId).OrderByDescending(x => x.CreatedAt).Take(3).ToListAsync(ct);

        var kpis = new List<DashboardKpiDto>
        {
            new("Pending quick chats", pendingChats.ToString(), pendingChats > 0 ? "Needs review" : "Queue clear", pendingChats > 0 ? "warning" : "success", "bi-chat-left-text"),
            new("Direct bookings", pendingBookings.ToString(), pendingBookings > 0 ? "Awaiting action" : "No pending bookings", pendingBookings > 0 ? "info" : "success", "bi-calendar-check"),
            new("Sessions this month", sessionsThisMonth.ToString(), "Filed session reports", "primary", "bi-clipboard2-pulse"),
            new("Avg mood improvement", $"+{Math.Round(avgMood, 1)}", "Across submitted reports", avgMood >= 2 ? "success" : "warning", "bi-graph-up-arrow")
        };

        var loadPercent = Math.Min(100, (pendingChats * 12) + (pendingBookings * 10));
        var note = loadPercent > 70 ? "High workload: prioritize SLA-risk quick chats first." : "Workload is healthy. Keep availability on if you can accept more users.";
        var now = DateTimeOffset.UtcNow;
        return new DashboardSummaryDto(kpis, recentChats.Select(x => x.ToDto(doctorId, now)).ToList(), recentBookings.Select(x => x.ToDto()).ToList(), latestReports.Select(x => x.ToDto(now)).ToList(), profile.IsAvailable, loadPercent, note);
    }
}
