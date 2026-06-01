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
[Route("api/v1/session-reports")]
public sealed class ApiSessionReportsController(ISessionReportService reports, ICurrentDoctorAccessor currentDoctor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var (items, total) = await reports.GetReportsAsync(currentDoctor.DoctorId, page, pageSize, ct);
        return Ok(ApiResponse.Paged(items, page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await reports.GetByIdAsync(currentDoctor.DoctorId, id, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSessionReportRequest request, CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await reports.CreateAsync(currentDoctor.DoctorId, request, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateSessionReportRequest request, CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await reports.UpdateAsync(currentDoctor.DoctorId, id, request, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }
}
