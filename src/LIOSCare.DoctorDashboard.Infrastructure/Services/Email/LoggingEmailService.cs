using LIOSCare.DoctorDashboard.Application.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services.Email;

public sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var plain = Regex.Replace(htmlBody, "<[^>]+>", " ")
                         .Replace("  ", " ").Trim();
        logger.LogInformation(
            "[DEV EMAIL] To={To} | Subject={Subject} | Body={Body}",
            to, subject, plain);
        return Task.CompletedTask;
    }
}
