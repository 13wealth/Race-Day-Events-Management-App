using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace RaceDay.API.Storage
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        public async Task<string> UploadProfileImageAsync(
            IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Please select an image.");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("Image size cannot exceed 5 MB.");
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Only JPG, PNG and WEBP images are allowed.");
            }

            string containerName =
                _configuration["AzureBlobStorage:ContainerName"];

            BlobContainerClient containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();                             // Ensure the container exists

            string fileName = $"profiles/{userId}/{Guid.NewGuid()}{extension}";

            BlobClient blobClient =
                containerClient.GetBlobClient(fileName);

            BlobUploadOptions uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            };

            using Stream stream = file.OpenReadStream();

            await blobClient.UploadAsync(stream, uploadOptions);

            return blobClient.Uri.AbsoluteUri;
        }
    }
}