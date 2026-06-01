namespace LIOSCare.DoctorDashboard.Infrastructure.Http;

public sealed class MessagingServiceOptions
{
    public string BaseUrl { get; set; } = "https://localhost:7081";
    public bool Enabled { get; set; } = false;
    public string ServiceToken { get; set; } = string.Empty;
}
