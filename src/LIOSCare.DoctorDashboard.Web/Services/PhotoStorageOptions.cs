namespace LIOSCare.DoctorDashboard.Web.Services;

public sealed class PhotoStorageOptions
{
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string PublicUrlBase { get; set; } = "/uploads/doctors";
}
