using LIOSCare.DoctorDashboard.Application.DTOs;

namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface IQuickChatService
{
    Task<IReadOnlyList<QuickChatDto>> GetQueueAsync(Guid doctorId, QuickChatQueueFilter filter, CancellationToken ct = default);
    Task<IReadOnlyList<QuickChatDto>> GetHistoryAsync(Guid doctorId, CancellationToken ct = default);
    Task<QuickChatDto> GetByIdAsync(Guid doctorId, Guid id, CancellationToken ct = default);
    Task<AcceptQuickChatResult> AcceptAsync(Guid doctorId, Guid id, CancellationToken ct = default);
    Task<QuickChatDto> ReplyAsync(Guid doctorId, Guid id, string reply, CancellationToken ct = default);
}
