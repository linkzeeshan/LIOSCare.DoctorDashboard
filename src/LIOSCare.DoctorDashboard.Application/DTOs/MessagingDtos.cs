namespace LIOSCare.DoctorDashboard.Application.DTOs;

public sealed record MessagingThreadDto(Guid ThreadId, string ThreadType, DateTimeOffset? LastMessageAt);
public sealed record MessagingMessageDto(Guid MessageId, Guid ThreadId, Guid SenderId, string MessageType, string? Body, DateTimeOffset CreatedAt);
public sealed record SendMessagingMessageRequest(Guid ThreadId, string MessageType, string Body);
