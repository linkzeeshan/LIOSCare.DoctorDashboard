namespace LIOSCare.DoctorDashboard.Application.DTOs;

public sealed record ServiceTierDto(Guid Id, string Name, decimal PriceUsd, string[] Features, int ResponseWindowHours, bool IsActive);
