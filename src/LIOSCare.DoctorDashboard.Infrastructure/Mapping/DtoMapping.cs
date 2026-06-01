using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Domain.Entities;

namespace LIOSCare.DoctorDashboard.Infrastructure.Mapping;

internal static class DtoMapping
{
    public static DoctorUserDto ToDoctorUser(this DoctorProfile profile, string email) =>
        new(profile.Id, profile.DoctorAccountId, email, profile.FullName, profile.Specializations);

    public static DoctorProfileDto ToDto(this DoctorProfile profile, string email) =>
        new(profile.Id, profile.FullName, email, profile.Specializations, profile.YearsExperience,
            profile.Rating, profile.PatientCount, profile.Bio, profile.Certifications,
            profile.Education.Select(e => new EducationDto(e.Id, e.Degree, e.Institution, e.Year)).ToList(),
            profile.ProfilePhotoUrl, profile.IsAvailable);

    public static ServiceTierDto ToDto(this ServiceTier tier) =>
        new(tier.Id, tier.Name, tier.PriceUsd, tier.Features, tier.ResponseWindowHours, tier.IsActive);

    public static QuickChatDto ToDto(this QuickChatRequest chat, Guid currentDoctorId, DateTimeOffset now)
    {
        var remaining = chat.SlaDeadline - now;
        var tone = remaining.TotalHours < 2 ? "danger" : remaining.TotalHours < 6 ? "warning" : "success";
        var slaText = remaining.TotalMinutes <= 0 ? "SLA overdue" : $"{Math.Floor(remaining.TotalHours)}h {remaining.Minutes}m left";
        return new QuickChatDto(chat.Id, chat.UserId, $"{chat.UserFirstName} {chat.UserLastInitial}.", chat.DoctorId,
            chat.TierId, chat.Tier?.Name ?? "Unknown", chat.Tier?.PriceUsd ?? 0,
            chat.Status.ToString(), chat.UserMessage, chat.DoctorReply, chat.MessagingThreadId, chat.CreatedAt,
            chat.AcceptedAt, chat.RepliedAt, chat.SlaDeadline, tone, slaText, chat.DoctorId == currentDoctorId);
    }

    public static BookingDto ToDto(this DirectBookingRequest booking) =>
        new(booking.Id, booking.UserId, booking.UserFullName, booking.UserPhotoUrl, booking.DoctorId, booking.TierId,
            booking.Tier?.Name ?? "Unknown", booking.AmountUsd, booking.PlatformFeeUsd, booking.Status.ToString(),
            booking.PaymentStatus.ToString(), booking.ScheduledAt, booking.Notes, booking.CreatedAt,
            booking.DeclineReason == LIOSCare.DoctorDashboard.Domain.Enums.DeclineReason.None ? null : booking.DeclineReason.ToString(),
            string.Equals(booking.Tier?.Name, "Diamond", StringComparison.OrdinalIgnoreCase));

    public static SessionReportDto ToDto(this SessionReport report, DateTimeOffset now) =>
        new(report.Id, report.BookingId, report.DoctorId, report.UserId, report.UserFullName, report.SessionNumber,
            report.SessionDate, report.MoodBefore, report.MoodAfter, report.GoalsCompleted, report.GoalsTotal,
            report.ProgressNotes, report.TechniquesUsed, report.NextSteps, report.CreatedAt, report.CanEdit(now));
}
