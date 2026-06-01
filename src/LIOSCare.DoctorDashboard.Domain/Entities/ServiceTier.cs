namespace LIOSCare.DoctorDashboard.Domain.Entities;

public sealed class ServiceTier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceUsd { get; set; }
    public string[] Features { get; set; } = Array.Empty<string>();
    public int ResponseWindowHours { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}
