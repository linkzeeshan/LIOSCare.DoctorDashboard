using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers;

[Authorize(Policy = "DoctorOnly")]
[Route("dashboard/session-reports")]
public sealed class SessionReportsController(ISessionReportService reports, IBookingService bookings, ICurrentDoctorAccessor currentDoctor) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var (items, _) = await reports.GetReportsAsync(currentDoctor.DoctorId, ct: ct);
        return View(items);
    }

    [HttpGet("new")]
    public async Task<IActionResult> New(CancellationToken ct)
    {
        var (eligible, _) = await bookings.GetBookingsAsync(currentDoctor.DoctorId, "eligible", ct: ct);
        ViewBag.Bookings = eligible;
        return View(new CreateSessionReportRequest(Guid.Empty, DateTime.UtcNow.Date, 5, 7, 1, 3, string.Empty, Array.Empty<string>(), string.Empty));
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(Guid bookingId, DateTime sessionDate, int moodBefore, int moodAfter, int goalsCompleted, int goalsTotal, string progressNotes, string techniquesUsed, string? nextSteps, CancellationToken ct)
    {
        try
        {
            var created = await reports.CreateAsync(currentDoctor.DoctorId, new CreateSessionReportRequest(bookingId, sessionDate, moodBefore, moodAfter, goalsCompleted, goalsTotal, progressNotes, Split(techniquesUsed), nextSteps), ct);
            TempData["Toast"] = "Session report saved. It is now visible to the patient as Private & Confidential.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (AppException ex)
        {
            TempData["ToastError"] = ex.Message;
            var (eligible, _) = await bookings.GetBookingsAsync(currentDoctor.DoctorId, "eligible", ct: ct);
            ViewBag.Bookings = eligible;
            return View(new CreateSessionReportRequest(bookingId, sessionDate, moodBefore, moodAfter, goalsCompleted, goalsTotal, progressNotes, Split(techniquesUsed), nextSteps));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct) => View(await reports.GetByIdAsync(currentDoctor.DoctorId, id, ct));

    [HttpGet("{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var report = await reports.GetByIdAsync(currentDoctor.DoctorId, id, ct);
        if (!report.IsEditable)
        {
            TempData["ToastError"] = "This report is older than 24 hours and cannot be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }
        return View(report);
    }

    [HttpPost("{id:guid}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DateTime sessionDate, int moodBefore, int moodAfter, int goalsCompleted, int goalsTotal, string progressNotes, string techniquesUsed, string? nextSteps, CancellationToken ct)
    {
        try
        {
            await reports.UpdateAsync(currentDoctor.DoctorId, id, new UpdateSessionReportRequest(sessionDate, moodBefore, moodAfter, goalsCompleted, goalsTotal, progressNotes, Split(techniquesUsed), nextSteps), ct);
            TempData["Toast"] = "Session report updated.";
        }
        catch (AppException ex) { TempData["ToastError"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }

    private static string[] Split(string? csv) => (csv ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToArray();
}
