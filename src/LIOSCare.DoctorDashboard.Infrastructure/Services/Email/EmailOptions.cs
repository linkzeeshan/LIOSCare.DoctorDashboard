namespace LIOSCare.DoctorDashboard.Infrastructure.Services.Email;

public sealed class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "noreply@lioscare.com";
    public string FromName { get; set; } = "LIOS Care";
}

public sealed class PasswordResetOptions
{
    public int TokenExpiryMinutes { get; set; } = 60;
    public string ResetUrlBase { get; set; } = "https://localhost:61130";
}
