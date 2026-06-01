using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Domain.Entities;
using LIOSCare.DoctorDashboard.Domain.Enums;
using LIOSCare.DoctorDashboard.Infrastructure.Mapping;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services;

public sealed class BookingService(DoctorPortalDbContext db, INotificationClient notificationClient) : IBookingService
{
    public async Task<(IReadOnlyList<BookingDto> Items, int Total)> GetBookingsAsync(Guid doctorId, string status = "pending", int page = 1, int pageSize = 100, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var query = db.DirectBookingRequests.Include(x => x.Tier).AsNoTracking().Where(x => x.DoctorId == doctorId);
        if (string.Equals(status, "eligible", StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => x.Status == BookingStatus.Accepted || x.Status == BookingStatus.Completed);
        else if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase) && Enum.TryParse<BookingStatus>(status, true, out var parsed))
            query = query.Where(x => x.Status == parsed);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (rows.Select(x => x.ToDto()).ToList(), total);
    }

    public async Task<BookingDto> GetByIdAsync(Guid doctorId, Guid id, CancellationToken ct = default)
    {
        var booking = await db.DirectBookingRequests.Include(x => x.Tier).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.DoctorId == doctorId, ct)
            ?? throw new AppException("Booking request not found.", 404);
        return booking.ToDto();
    }

    public async Task<BookingDto> AcceptAsync(Guid doctorId, Guid id, CancellationToken ct = default)
    {
        var booking = await LoadOwnedAsync(doctorId, id, ct);
        if (booking.Status != BookingStatus.Pending) throw new AppException("Only pending bookings can be accepted.");
        booking.Status = BookingStatus.Accepted;
        booking.AcceptedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        await notificationClient.NotifyUserAsync(booking.UserId, "booking_accepted", "Booking accepted", "Your doctor accepted your booking request.", ct);
        return booking.ToDto();
    }

    public async Task<BookingDto> DeclineAsync(Guid doctorId, Guid id, DeclineBookingRequest request, CancellationToken ct = default)
    {
        var booking = await LoadOwnedAsync(doctorId, id, ct);
        if (booking.Status != BookingStatus.Pending) throw new AppException("Only pending bookings can be declined.");
        if (!Enum.TryParse<DeclineReason>(request.Reason.Replace(" ", string.Empty), true, out var reason) || reason == DeclineReason.None)
            throw new AppException("A valid decline reason is required.");
        booking.Status = BookingStatus.Declined;
        booking.DeclineReason = reason;
        booking.DeclineNote = request.Note;
        booking.DeclinedAt = DateTimeOffset.UtcNow;
        booking.PaymentStatus = PaymentStatus.RefundPending;
        await db.SaveChangesAsync(ct);
        await notificationClient.NotifyUserAsync(booking.UserId, "booking_declined", "Booking declined", "Your booking was declined. A refund has been queued and will be processed shortly.", ct);
        return booking.ToDto();
    }

    public async Task<BookingDto> ScheduleAsync(Guid doctorId, Guid id, ScheduleBookingRequest request, CancellationToken ct = default)
    {
        var booking = await LoadOwnedAsync(doctorId, id, ct);
        if (!string.Equals(booking.Tier?.Name, "Diamond", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Scheduling is only available for Diamond tier bookings.", 400);
        if (booking.Status != BookingStatus.Accepted)
            throw new AppException("Accept the booking before scheduling it.", 400);
        if (request.ScheduledAt <= DateTimeOffset.UtcNow)
            throw new AppException("Scheduled time must be in the future.", 400);
        booking.ScheduledAt = request.ScheduledAt;
        await db.SaveChangesAsync(ct);
        await notificationClient.NotifyUserAsync(booking.UserId, "booking_scheduled", "Session scheduled", "Your LIOS Care session time has been scheduled.", ct);
        return booking.ToDto();
    }

    private async Task<DirectBookingRequest> LoadOwnedAsync(Guid doctorId, Guid id, CancellationToken ct) =>
        await db.DirectBookingRequests.Include(x => x.Tier)
            .FirstOrDefaultAsync(x => x.Id == id && x.DoctorId == doctorId, ct)
        ?? throw new AppException("Booking request not found.", 404);
}
