using System.Net.Http.Headers;
using System.Net.Http.Json;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LIOSCare.DoctorDashboard.Infrastructure.Http;

public sealed class HttpMessagingServiceClient(HttpClient http, IOptions<MessagingServiceOptions> options, ILogger<HttpMessagingServiceClient> logger) : IMessagingServiceClient
{
    private readonly MessagingServiceOptions _options = options.Value;

    public async Task<IReadOnlyList<MessagingThreadDto>> GetThreadsAsync(Guid doctorId, CancellationToken ct = default)
    {
        if (!_options.Enabled) return Array.Empty<MessagingThreadDto>();
        try
        {
            ApplyAuth();
            var result = await http.GetFromJsonAsync<List<MessagingThreadDto>>("api/v1/threads", ct);
            return result ?? new List<MessagingThreadDto>();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Messaging service thread list failed.");
            return Array.Empty<MessagingThreadDto>();
        }
    }

    public async Task<IReadOnlyList<MessagingMessageDto>> GetThreadMessagesAsync(Guid doctorId, Guid threadId, CancellationToken ct = default)
    {
        if (!_options.Enabled) return Array.Empty<MessagingMessageDto>();
        try
        {
            ApplyAuth();
            var result = await http.GetFromJsonAsync<List<MessagingMessageDto>>($"api/v1/threads/{threadId}/messages", ct);
            return result ?? new List<MessagingMessageDto>();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Messaging service message list failed for thread {ThreadId}.", threadId);
            return Array.Empty<MessagingMessageDto>();
        }
    }

    public async Task<MessagingMessageDto?> SendMessageAsync(Guid doctorId, SendMessagingMessageRequest request, CancellationToken ct = default)
    {
        if (!_options.Enabled) return null;
        try
        {
            ApplyAuth();
            var response = await http.PostAsJsonAsync($"api/v1/threads/{request.ThreadId}/messages", new { messageType = request.MessageType, body = request.Body }, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<MessagingMessageDto>(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Messaging service send message failed for thread {ThreadId}.", request.ThreadId);
            return null;
        }
    }

    private void ApplyAuth()
    {
        if (!string.IsNullOrWhiteSpace(_options.ServiceToken))
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceToken);
        }
    }
}
