using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Web.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers.Api;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "doctor")]
[Route("api/v1/bookings")]
public sealed class ApiBookingsController(IBookingService bookings, ICurrentDoctorAccessor currentDoctor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(string status = "pending", int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var (items, total) = await bookings.GetBookingsAsync(currentDoctor.DoctorId, status, page, pageSize, ct);
        return Ok(ApiResponse.Paged(items, page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await bookings.GetByIdAsync(currentDoctor.DoctorId, id, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await bookings.AcceptAsync(currentDoctor.DoctorId, id, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id, DeclineBookingRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(ApiResponse.Err("A decline reason is required.", 400));
        try { return Ok(ApiResponse.Data(await bookings.DeclineAsync(currentDoctor.DoctorId, id, request, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpPatch("{id:guid}/schedule")]
    public async Task<IActionResult> Schedule(Guid id, ScheduleBookingRequest request, CancellationToken ct)
    {
        if (request.ScheduledAt <= DateTimeOffset.UtcNow)
            return BadRequest(ApiResponse.Err("Scheduled time must be in the future.", 400));
        try { return Ok(ApiResponse.Data(await bookings.ScheduleAsync(currentDoctor.DoctorId, id, request, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }
}
