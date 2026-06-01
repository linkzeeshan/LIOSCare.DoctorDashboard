using System.Net.Http.Json;
using LIOSCare.DoctorDashboard.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LIOSCare.DoctorDashboard.Infrastructure.Http;

public sealed class HttpNotificationClient(HttpClient http, IOptions<NotificationServiceOptions> options, ILogger<HttpNotificationClient> logger) : INotificationClient
{
    private readonly NotificationServiceOptions _options = options.Value;

    public async Task NotifyUserAsync(Guid userId, string type, string title, string body, CancellationToken ct = default)
    {
        if (!_options.Enabled) return;
        try
        {
            if (!string.IsNullOrWhiteSpace(_options.InternalApiKey))
                http.DefaultRequestHeaders.TryAddWithoutValidation("X-Internal-Api-Key", _options.InternalApiKey);
            await http.PostAsJsonAsync("api/v1/internal/dispatch", new { recipientId = userId, type, title, body }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Notification dispatch failed for user {UserId}.", userId);
        }
    }
}
