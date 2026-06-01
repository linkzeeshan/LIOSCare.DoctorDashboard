using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers;

[Authorize(Policy = "DoctorOnly")]
[Route("dashboard/bookings")]
public sealed class BookingsController(IBookingService bookings, ICurrentDoctorAccessor currentDoctor) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string status = "pending", CancellationToken ct = default)
    {
        ViewBag.Status = status;
        var (items, _) = await bookings.GetBookingsAsync(currentDoctor.DoctorId, status, ct: ct);
        return View(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct) => View(await bookings.GetByIdAsync(currentDoctor.DoctorId, id, ct));

    [HttpPost("{id:guid}/accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        try
        {
            await bookings.AcceptAsync(currentDoctor.DoctorId, id, ct);
            TempData["Toast"] = "Booking accepted. User notification triggered.";
        }
        catch (AppException ex) { TempData["ToastError"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:guid}/decline")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(Guid id, string reason, string? note, CancellationToken ct)
    {
        try
        {
            await bookings.DeclineAsync(currentDoctor.DoctorId, id, new DeclineBookingRequest(reason, note), ct);
            TempData["Toast"] = "Booking declined. Refund workflow has been marked for backend processing.";
        }
        catch (AppException ex) { TempData["ToastError"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:guid}/schedule")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(Guid id, DateTimeOffset scheduledAt, CancellationToken ct)
    {
        try
        {
            await bookings.ScheduleAsync(currentDoctor.DoctorId, id, new ScheduleBookingRequest(scheduledAt), ct);
            TempData["Toast"] = "Diamond session scheduled and user notification triggered.";
        }
        catch (AppException ex) { TempData["ToastError"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }
}
