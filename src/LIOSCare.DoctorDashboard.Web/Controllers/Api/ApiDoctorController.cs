using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Web.Models;
using LIOSCare.DoctorDashboard.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers.Api;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "doctor")]
[Route("api/v1/doctors/me")]
public sealed class ApiDoctorController(
    IDoctorProfileService profiles,
    ICurrentDoctorAccessor currentDoctor,
    IPhotoStorageService photoStorage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await profiles.GetMeAsync(currentDoctor.DoctorId, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpPatch]
    public async Task<IActionResult> Patch(UpdateDoctorProfileRequest request, CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await profiles.UpdateMeAsync(currentDoctor.DoctorId, request, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await profiles.GetStatsAsync(currentDoctor.DoctorId, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto([FromForm] IFormFile? photo, CancellationToken ct)
    {
        if (photo is null || photo.Length == 0)
            return BadRequest(ApiResponse.Err("A photo file is required.", 400));
        try
        {
            var profile = await profiles.GetMeAsync(currentDoctor.DoctorId, ct);
            var oldUrl = profile.ProfilePhotoUrl;
            var newUrl = await photoStorage.SaveAsync(photo, currentDoctor.DoctorId, ct);
            await profiles.SetProfilePhotoAsync(currentDoctor.DoctorId, newUrl, ct);
            await photoStorage.TryDeleteAsync(oldUrl, ct);
            return Ok(ApiResponse.Data(new { profilePhotoUrl = newUrl }));
        }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }
}
