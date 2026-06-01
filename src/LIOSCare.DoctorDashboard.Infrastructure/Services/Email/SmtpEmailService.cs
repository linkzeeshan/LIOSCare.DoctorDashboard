using LIOSCare.DoctorDashboard.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace LIOSCare.DoctorDashboard.Infrastructure.Services.Email;

public sealed class SmtpEmailService(
    IOptions<EmailOptions> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailOptions _opts = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            using var client = new SmtpClient(_opts.SmtpHost, _opts.SmtpPort)
            {
                EnableSsl = _opts.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_opts.SmtpUser))
                client.Credentials = new NetworkCredential(_opts.SmtpUser, _opts.SmtpPassword);

            using var message = new MailMessage
            {
                From = new MailAddress(_opts.FromAddress, _opts.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To} with subject {Subject}.", to, subject);
        }
    }
}
