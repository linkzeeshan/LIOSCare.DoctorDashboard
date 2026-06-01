using LIOSCare.DoctorDashboard.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers;

[Authorize(Policy = "DoctorOnly")]
[Route("dashboard")]
public sealed class DashboardController(IDashboardQueryService dashboardQuery, IDoctorProfileService profileService, ICurrentDoctorAccessor currentDoctor) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = await dashboardQuery.GetDashboardAsync(currentDoctor.DoctorId, ct);
        return View(vm);
    }

    [HttpPost("availability")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Availability(bool isAvailable, CancellationToken ct)
    {
        await profileService.SetAvailabilityAsync(currentDoctor.DoctorId, isAvailable, ct);
        TempData["Toast"] = isAvailable ? "Availability is on. You can receive quick chat pool requests." : "Availability is off. General pool requests are paused.";
        return RedirectToAction(nameof(Index));
    }
}
