namespace LIOSCare.DoctorDashboard.Infrastructure.Http;

public sealed class NotificationServiceOptions
{
    public string BaseUrl { get; set; } = "https://localhost:7181";
    public bool Enabled { get; set; } = false;
    public string InternalApiKey { get; set; } = string.Empty;
}
