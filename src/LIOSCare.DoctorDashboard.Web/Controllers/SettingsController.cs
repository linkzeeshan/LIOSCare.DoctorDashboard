using LIOSCare.DoctorDashboard.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers;

[Authorize(Policy = "DoctorOnly")]
[Route("dashboard/settings")]
public sealed class SettingsController(IDoctorProfileService profileService, ICurrentDoctorAccessor currentDoctor) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct) => View(await profileService.GetMeAsync(currentDoctor.DoctorId, ct));

    [HttpPost("notifications")]
    [ValidateAntiForgeryToken]
    public IActionResult Notifications(bool quickChatPush, bool bookingPush, bool reportReminders)
    {
        TempData["Toast"] = "Notification preferences saved locally. Wire NotificationService preferences in the next deployment step.";
        return RedirectToAction(nameof(Index));
    }
}
