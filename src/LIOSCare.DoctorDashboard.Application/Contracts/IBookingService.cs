using LIOSCare.DoctorDashboard.Application.DTOs;

namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface IBookingService
{
    Task<(IReadOnlyList<BookingDto> Items, int Total)> GetBookingsAsync(Guid doctorId, string status = "pending", int page = 1, int pageSize = 100, CancellationToken ct = default);
    Task<BookingDto> GetByIdAsync(Guid doctorId, Guid id, CancellationToken ct = default);
    Task<BookingDto> AcceptAsync(Guid doctorId, Guid id, CancellationToken ct = default);
    Task<BookingDto> DeclineAsync(Guid doctorId, Guid id, DeclineBookingRequest request, CancellationToken ct = default);
    Task<BookingDto> ScheduleAsync(Guid doctorId, Guid id, ScheduleBookingRequest request, CancellationToken ct = default);
}
