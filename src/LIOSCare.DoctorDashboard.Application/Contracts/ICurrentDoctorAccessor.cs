namespace LIOSCare.DoctorDashboard.Application.Contracts;

public interface ICurrentDoctorAccessor
{
    Guid DoctorId { get; }
    Guid AccountId { get; }
    string Email { get; }
}
