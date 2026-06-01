using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Web.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers.Api;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "doctor")]
[Route("api/v1/service-tiers")]
public sealed class ApiServiceTiersController(IServiceTierService tiers) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var items = await tiers.GetActiveAsync(ct);
        return Ok(ApiResponse.Listed(items));
    }

    [HttpPatch("{id:guid}")]
    public IActionResult ForbiddenPatch(Guid id) => Forbid();
}
