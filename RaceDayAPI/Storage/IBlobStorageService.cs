using Microsoft.AspNetCore.Http;

namespace RaceDay.API.Storage
{
    public interface IBlobStorageService
    {
        Task<string> UploadProfileImageAsync(IFormFile file, int userId);

        Task<(
              byte[]? Content, 
              string? ContentType
        )> 
            DownloadProfileImageAsync(string imageUrl);
    }
}