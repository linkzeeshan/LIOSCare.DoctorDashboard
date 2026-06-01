using LIOSCare.DoctorDashboard.Application.Common;
using Microsoft.Extensions.Options;

namespace LIOSCare.DoctorDashboard.Web.Services;

public sealed class LocalPhotoStorageService(
    IWebHostEnvironment env,
    IOptions<PhotoStorageOptions> options) : IPhotoStorageService
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/jpg", "image/png" };

    private readonly PhotoStorageOptions _opts = options.Value;

    public async Task<string> SaveAsync(IFormFile file, Guid doctorId, CancellationToken ct = default)
    {
        ValidateFile(file);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{doctorId:N}-{Guid.NewGuid():N}{ext}";

        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", "doctors");
        Directory.CreateDirectory(uploadsDir);

        var fullPath = Path.Combine(uploadsDir, fileName);
        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, ct);

        return $"{_opts.PublicUrlBase.TrimEnd('/')}/{fileName}";
    }

    public Task TryDeleteAsync(string? publicUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(publicUrl)) return Task.CompletedTask;
        if (!publicUrl.StartsWith(_opts.PublicUrlBase, StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var relativePath = publicUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(env.WebRootPath, relativePath);
        if (File.Exists(fullPath))
        {
            try { File.Delete(fullPath); }
            catch { }
        }
        return Task.CompletedTask;
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
            throw new AppException("The selected file is empty.", 400);

        var ext = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext) || !AllowedContentTypes.Contains(file.ContentType))
            throw new AppException("Only JPG and PNG images are accepted.", 400);

        if (file.Length > _opts.MaxFileSizeBytes)
            throw new AppException($"Photo must be smaller than {_opts.MaxFileSizeBytes / 1024 / 1024} MB.", 400);
    }
}
