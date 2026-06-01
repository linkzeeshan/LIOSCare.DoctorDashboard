namespace LIOSCare.DoctorDashboard.Application.DTOs;

public sealed record DashboardKpiDto(string Label, string Value, string SubText, string Tone, string Icon);
public sealed record DashboardSummaryDto(
    IReadOnlyList<DashboardKpiDto> Kpis,
    IReadOnlyList<QuickChatDto> RecentQuickChats,
    IReadOnlyList<BookingDto> RecentBookings,
    IReadOnlyList<SessionReportDto> LatestReports,
    bool IsAvailable,
    int CareLoadPercent,
    string AttentionNote);
