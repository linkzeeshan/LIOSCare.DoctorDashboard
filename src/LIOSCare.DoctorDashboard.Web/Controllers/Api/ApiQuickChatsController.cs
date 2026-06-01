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
[Route("api/v1/quick-chats")]
public sealed class ApiQuickChatsController(IQuickChatService chats, ICurrentDoctorAccessor currentDoctor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Queue(string status = "pending", string tier = "all", string type = "all", string sort = "sla", CancellationToken ct = default)
    {
        var items = await chats.GetQueueAsync(currentDoctor.DoctorId, new QuickChatQueueFilter(status, tier, type, sort), ct);
        return Ok(ApiResponse.Listed(items));
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(CancellationToken ct)
    {
        var items = await chats.GetHistoryAsync(currentDoctor.DoctorId, ct);
        return Ok(ApiResponse.Listed(items));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        try { return Ok(ApiResponse.Data(await chats.GetByIdAsync(currentDoctor.DoctorId, id, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        var result = await chats.AcceptAsync(currentDoctor.DoctorId, id, ct);
        return result.Success
            ? Ok(ApiResponse.Data(result))
            : StatusCode(result.HttpStatusCode, ApiResponse.Err(result.Message, result.HttpStatusCode));
    }

    [HttpPost("{id:guid}/reply")]
    public async Task<IActionResult> Reply(Guid id, ReplyQuickChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reply))
            return BadRequest(ApiResponse.Err("Reply cannot be empty.", 400));
        try { return Ok(ApiResponse.Data(await chats.ReplyAsync(currentDoctor.DoctorId, id, request.Reply, ct))); }
        catch (AppException ex) { return StatusCode(ex.StatusCode, ApiResponse.Err(ex.Message, ex.StatusCode)); }
    }
}
