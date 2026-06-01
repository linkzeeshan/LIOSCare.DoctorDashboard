namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface INotificationClient
{
    Task NotifyUserAsync(Guid userId, string type, string title, string body, CancellationToken ct = default);
}
