using whobe.Interfaces;

namespace whobe.Services;
//upload file vào wwwroot/uploads, validate size/type
public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] _allowedImages = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private static readonly string[] _allowedVideos = { ".mp4", ".mov", ".avi", ".mkv", ".webm" };
    private const long MaxImageSize = 10 * 1024 * 1024;  // 10 MB
    private const long MaxVideoSize = 200 * 1024 * 1024; // 200 MB

    public FileStorageService(IConfiguration config, IWebHostEnvironment env) {
        _config = config;
        _env    = env;
    }

    public async Task<string> UploadAsync(IFormFile file, string folder) {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var dirPath = Path.Combine(_env.WebRootPath, "uploads", folder);

        Directory.CreateDirectory(dirPath);

        var fullPath = Path.Combine(dirPath, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        var baseUrl = _config["FileStorage:BaseUrl"]?.TrimEnd('/');
        return $"{baseUrl}/uploads/{folder}/{fileName}";
    }

    public async Task<IEnumerable<string>> UploadManyAsync(
        IEnumerable<IFormFile> files, string folder) {
        var urls = new List<string>();
        foreach (var file in files)
            urls.Add(await UploadAsync(file, folder));
        return urls;
    }

    public Task DeleteAsync(string fileUrl) {
        try {
            var baseUrl  = _config["FileStorage:BaseUrl"]?.TrimEnd('/') ?? "";
            var relative = fileUrl.Replace(baseUrl, "").TrimStart('/');
            var fullPath = Path.Combine(_env.WebRootPath, relative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch { /* ignore nếu file không tồn tại */ }
        return Task.CompletedTask;
    }

    public bool IsValidImage(IFormFile file) =>
        _allowedImages.Contains(Path.GetExtension(file.FileName).ToLowerInvariant())
        && file.Length <= MaxImageSize;

    public bool IsValidVideo(IFormFile file) =>
        _allowedVideos.Contains(Path.GetExtension(file.FileName).ToLowerInvariant())
        && file.Length <= MaxVideoSize;
}
