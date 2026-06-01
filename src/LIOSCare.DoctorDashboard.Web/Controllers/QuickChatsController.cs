using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LIOSCare.DoctorDashboard.Web.Controllers;

[Authorize(Policy = "DoctorOnly")]
[Route("dashboard/quick-chats")]
public sealed class QuickChatsController(
    IQuickChatService chats,
    IDoctorProfileService profiles,
    ICurrentDoctorAccessor currentDoctor) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string tier = "all", string type = "all", string sort = "sla", CancellationToken ct = default)
    {
        var profile = await profiles.GetMeAsync(currentDoctor.DoctorId, ct);
        var rows = await chats.GetQueueAsync(currentDoctor.DoctorId, new QuickChatQueueFilter("pending", tier, type, sort), ct);
        ViewBag.Tier = tier;
        ViewBag.Type = type;
        ViewBag.Sort = sort;
        ViewBag.DoctorIsAvailable = profile.IsAvailable;
        return View(rows);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var chat = await chats.GetByIdAsync(currentDoctor.DoctorId, id, ct);
        return View(chat);
    }

    [HttpPost("{id:guid}/accept")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        var result = await chats.AcceptAsync(currentDoctor.DoctorId, id, ct);
        if (result.Success)
        {
            TempData["Toast"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
        TempData["ToastError"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:guid}/reply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(Guid id, string reply, CancellationToken ct)
    {
        try
        {
            await chats.ReplyAsync(currentDoctor.DoctorId, id, reply, ct);
            TempData["Toast"] = "Reply sent. User has been notified on mobile.";
        }
        catch (AppException ex)
        {
            TempData["ToastError"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(CancellationToken ct)
    {
        var rows = await chats.GetHistoryAsync(currentDoctor.DoctorId, ct);
        return View(rows);
    }
}
