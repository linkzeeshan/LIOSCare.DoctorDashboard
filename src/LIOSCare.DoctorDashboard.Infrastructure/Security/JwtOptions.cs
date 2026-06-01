namespace LIOSCare.DoctorDashboard.Infrastructure.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "lioscare-doctor-dashboard";
    public string Audience { get; set; } = "lioscare-doctor-dashboard";
    public string SigningKey { get; set; } = "CHANGE_ME_LOCAL_DEV_ONLY_MINIMUM_64_CHARACTERS_SIGNING_KEY";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
