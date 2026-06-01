namespace LIOSCare.DoctorDashboard.Application.DTOs;

public sealed record QuickChatDto(
    Guid Id,
    Guid UserId,
    string UserDisplayName,
    Guid? DoctorId,
    Guid TierId,
    string TierName,
    decimal TierPrice,
    string Status,
    string UserMessage,
    string? DoctorReply,
    Guid? MessagingThreadId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RepliedAt,
    DateTimeOffset SlaDeadline,
    string SlaTone,
    string SlaText,
    bool PickedYouSpecifically);

public sealed record QuickChatQueueFilter(string Status = "pending", string Tier = "all", string Type = "all", string Sort = "sla");
public sealed record ReplyQuickChatRequest(string Reply);
public sealed record AcceptQuickChatResult(bool Success, string Message, int HttpStatusCode = 200);
