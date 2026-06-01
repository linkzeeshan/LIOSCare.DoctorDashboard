using System.Security.Claims;
using LIOSCare.DoctorDashboard.Application.Contracts;
using LIOSCare.DoctorDashboard.Application.Common;

namespace LIOSCare.DoctorDashboard.Web.Security;

public sealed class CurrentDoctorAccessor(IHttpContextAccessor accessor) : ICurrentDoctorAccessor
{
    private ClaimsPrincipal User => accessor.HttpContext?.User ?? throw new AppException("No active HTTP context.", 401);
    public Guid DoctorId => ReadGuid("doctor_id", ClaimTypes.NameIdentifier);
    public Guid AccountId => ReadGuid("account_id", ClaimTypes.NameIdentifier);
    public string Email => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    private Guid ReadGuid(params string[] claimTypes)
    {
        foreach (var type in claimTypes)
        {
            var value = User.FindFirstValue(type);
            if (Guid.TryParse(value, out var id)) return id;
        }
        throw new AppException("Doctor identity is missing from the current session.", 401);
    }
}
