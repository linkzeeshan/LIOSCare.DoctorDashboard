using LIOSCare.DoctorDashboard.Domain.Enums;

namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class QuickChatRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastInitial { get; set; } = string.Empty;
    public Guid? DoctorId { get; set; }
    public Guid TierId { get; set; }
    public QuickChatStatus Status { get; set; } = QuickChatStatus.Pending;
    public string UserMessage { get; set; } = string.Empty;
    public string? DoctorReply { get; set; }
    public Guid? MessagingThreadId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RepliedAt { get; set; }
    public DateTimeOffset SlaDeadline { get; set; }

    public DoctorProfile? Doctor { get; set; }
    public ServiceTier? Tier { get; set; }
}
