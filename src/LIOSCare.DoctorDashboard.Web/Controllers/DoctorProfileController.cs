using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers;

[Authorize(Policy = "DoctorOnly")]
[Route("dashboard/profile")]
public sealed class DoctorProfileController(
    IDoctorProfileService profileService,
    IServiceTierService tierService,
    ICurrentDoctorAccessor currentDoctor,
    IPhotoStorageService photoStorage) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewBag.ServiceTiers = await tierService.GetActiveAsync(ct);
        return View(await profileService.GetMeAsync(currentDoctor.DoctorId, ct));
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string fullName, string specializations, int yearsExperience, string bio, string certifications, bool isAvailable, CancellationToken ct)
    {
        var request = new UpdateDoctorProfileRequest(
            fullName,
            Split(specializations),
            yearsExperience,
            bio,
            Split(certifications),
            ReadEducationFromForm(Request.Form),
            isAvailable);
        await profileService.UpdateMeAsync(currentDoctor.DoctorId, request, ct);
        TempData["Toast"] = "Profile updated. Changes will appear on the mobile doctor profile.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("photo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Photo(IFormFile? photo, CancellationToken ct)
    {
        if (photo is null || photo.Length == 0)
        {
            TempData["PhotoError"] = "Please select a photo to upload.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            var profile = await profileService.GetMeAsync(currentDoctor.DoctorId, ct);
            var oldUrl = profile.ProfilePhotoUrl;
            var newUrl = await photoStorage.SaveAsync(photo, currentDoctor.DoctorId, ct);
            await profileService.SetProfilePhotoAsync(currentDoctor.DoctorId, newUrl, ct);
            await photoStorage.TryDeleteAsync(oldUrl, ct);
            TempData["Toast"] = "Profile photo updated.";
        }
        catch (AppException ex)
        {
            TempData["PhotoError"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private static string[] Split(string? csv) => (csv ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToArray();

    private static List<EducationDto> ReadEducationFromForm(IFormCollection form)
    {
        var degrees = form["educationDegree"].ToArray();
        var institutions = form["educationInstitution"].ToArray();
        var years = form["educationYear"].ToArray();
        var rows = new List<EducationDto>();
        for (var i = 0; i < degrees.Length; i++)
        {
            var degree = degrees[i];
            var institution = institutions.ElementAtOrDefault(i);
            if (string.IsNullOrWhiteSpace(degree) || string.IsNullOrWhiteSpace(institution)) continue;
            int.TryParse(years.ElementAtOrDefault(i), out var year);
            rows.Add(new EducationDto(null, degree, institution, year == 0 ? DateTime.UtcNow.Year : year));
        }
        return rows;
    }
}
