using LIOSCare.DoctorDashboard.Application.Common;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using LIOSCare.DoctorDashboard.Domain.Enums;
using LIOSCare.DoctorDashboard.Infrastructure.Mapping;
using LIOSCare.DoctorDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services;

public sealed class QuickChatService(
    DoctorPortalDbContext db,
    IMessagingServiceClient messagingClient,
    INotificationClient notificationClient,
    IOptions<QuickChatOptions> options) : IQuickChatService
{
    private readonly QuickChatOptions _opts = options.Value;

    public async Task<IReadOnlyList<QuickChatDto>> GetQueueAsync(Guid doctorId, QuickChatQueueFilter filter, CancellationToken ct = default)
    {
        var isAvailable = await db.DoctorProfiles.AsNoTracking()
            .Where(x => x.Id == doctorId)
            .Select(x => x.IsAvailable)
            .FirstOrDefaultAsync(ct);

        var query = db.QuickChatRequests.Include(x => x.Tier).AsNoTracking()
            .Where(x => x.Status == QuickChatStatus.Pending &&
                (x.DoctorId == doctorId ||
                 (x.DoctorId == null && isAvailable)));

        if (!string.Equals(filter.Tier, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => x.Tier!.Name.ToLower() == filter.Tier.ToLower());

        query = filter.Type.ToLowerInvariant() switch
        {
            "for-me" => query.Where(x => x.DoctorId == doctorId),
            "pool"   => query.Where(x => x.DoctorId == null),
            _        => query
        };

        query = filter.Sort.ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(x => x.CreatedAt),
            "tier"   => query.OrderByDescending(x => x.Tier!.PriceUsd).ThenBy(x => x.SlaDeadline),
            _        => query.OrderBy(x => x.SlaDeadline)
        };

        var rows = await query.Take(100).ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        return rows.Select(x => x.ToDto(doctorId, now)).ToList();
    }

    public async Task<IReadOnlyList<QuickChatDto>> GetHistoryAsync(Guid doctorId, CancellationToken ct = default)
    {
        var rows = await db.QuickChatRequests.Include(x => x.Tier).AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.Status != QuickChatStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(ct);
        var now = DateTimeOffset.UtcNow;
        return rows.Select(x => x.ToDto(doctorId, now)).ToList();
    }

    public async Task<QuickChatDto> GetByIdAsync(Guid doctorId, Guid id, CancellationToken ct = default)
    {
        var chat = await db.QuickChatRequests.Include(x => x.Tier).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && (x.DoctorId == null || x.DoctorId == doctorId), ct)
            ?? throw new AppException("Quick chat request not found.", 404);
        return chat.ToDto(doctorId, DateTimeOffset.UtcNow);
    }

    public async Task<AcceptQuickChatResult> AcceptAsync(Guid doctorId, Guid id, CancellationToken ct = default)
    {
        var acceptedCount = await db.QuickChatRequests
            .CountAsync(x => x.DoctorId == doctorId && x.Status == QuickChatStatus.Accepted, ct);

        if (acceptedCount >= _opts.MaxConcurrentAcceptedChats)
            return new AcceptQuickChatResult(false,
                $"You already have {_opts.MaxConcurrentAcceptedChats} active quick chats. Reply to one before accepting another.", 400);

        var now = DateTimeOffset.UtcNow;
        var updated = await db.QuickChatRequests
            .Where(x => x.Id == id
                && x.Status == QuickChatStatus.Pending
                && (x.DoctorId == null || x.DoctorId == doctorId))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.DoctorId, doctorId)
                .SetProperty(x => x.Status, QuickChatStatus.Accepted)
                .SetProperty(x => x.AcceptedAt, (DateTimeOffset?)now), ct);

        return updated == 1
            ? new AcceptQuickChatResult(true,  "Quick chat accepted.", 200)
            : new AcceptQuickChatResult(false, "This request was already claimed by another doctor.", 409);
    }

    public async Task<QuickChatDto> ReplyAsync(Guid doctorId, Guid id, string reply, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reply))
            throw new AppException("Reply cannot be empty.", 400);

        var chat = await db.QuickChatRequests.Include(x => x.Tier)
            .FirstOrDefaultAsync(x => x.Id == id && x.DoctorId == doctorId, ct)
            ?? throw new AppException("Quick chat not found or not assigned to you.", 404);

        if (chat.Status is not QuickChatStatus.Accepted)
            throw new AppException(
                chat.Status is QuickChatStatus.Pending
                    ? "Accept this request before sending a reply."
                    : "This chat has already been replied to.", 400);

        chat.DoctorReply = reply.Trim();
        chat.RepliedAt = DateTimeOffset.UtcNow;
        chat.Status = QuickChatStatus.Replied;
        await db.SaveChangesAsync(ct);

        if (chat.MessagingThreadId.HasValue)
            await messagingClient.SendMessageAsync(doctorId, new SendMessagingMessageRequest(chat.MessagingThreadId.Value, "text", reply.Trim()), ct);

        await notificationClient.NotifyUserAsync(chat.UserId, "quick_chat_reply", "Doctor replied", "Your LIOS Care quick chat response is ready.", ct);
        return chat.ToDto(doctorId, DateTimeOffset.UtcNow);
    }
}
