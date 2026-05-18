namespace whobe.Interfaces;

public interface IFileStorageService {
    Task<string> UploadAsync(IFormFile file, string folder);
    Task<IEnumerable<string>> UploadManyAsync(IEnumerable<IFormFile> files, string folder);
    Task DeleteAsync(string fileUrl);
    bool IsValidImage(IFormFile file);
    bool IsValidVideo(IFormFile file);
}
