using LIOSCare.DoctorDashboard.Application.DTOs;

namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface IMessagingServiceClient
{
    Task<IReadOnlyList<MessagingThreadDto>> GetThreadsAsync(Guid doctorId, CancellationToken ct = default);
    Task<IReadOnlyList<MessagingMessageDto>> GetThreadMessagesAsync(Guid doctorId, Guid threadId, CancellationToken ct = default);
    Task<MessagingMessageDto?> SendMessageAsync(Guid doctorId, SendMessagingMessageRequest request, CancellationToken ct = default);
}
