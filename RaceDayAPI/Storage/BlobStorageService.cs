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
                                    IConfiguration configuration
            )
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }


        /*
         * Uploads a profile image to Azure Blob Storage.
         * 
         * @param file The image file to upload.
         * @param userId The ID of the user for whom the profile image is being uploaded.
         * @returns The URL of the uploaded image.
         * @throws ArgumentException If the file is null, empty, exceeds 5 MB, or has an invalid extension.
         */
        public async Task<string> UploadProfileImageAsync(
                                                            IFormFile file, 
                                                            int userId
            )
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

        public async Task<(
                            byte[]? Content, 
                            string? ContentType
                         )> 
                DownloadProfileImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return (null, null);

            string containerName =
                _configuration["AzureBlobStorage:ContainerName"];

            Uri imageUri = new Uri(imageUrl);

            string containerPath = "/" + containerName + "/";

            int containerIndex =
                imageUri.AbsolutePath.IndexOf(
                    containerPath,
                    StringComparison.OrdinalIgnoreCase);

            if (containerIndex < 0)
                return (null, null);

            string blobName = Uri.UnescapeDataString(
                imageUri.AbsolutePath.Substring(
                    containerIndex + containerPath.Length));

            BlobContainerClient containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                var response = await blobClient.DownloadContentAsync();

                return (
                    response.Value.Content.ToArray(),
                    response.Value.Details.ContentType
                );
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return (null, null);
            }
        }


        /*
         * Uploads an event banner image to Azure Blob Storage.
         * 
         * @param file The image file to upload.
         * @param eventId The ID of the event for which the banner is being uploaded.
         * @returns The URL of the uploaded image.
         * @throws ArgumentException If the file is null, empty, exceeds 5 MB, or has an invalid extension.
         */
        public async Task<string> UploadEventBannerAsync(IFormFile file, int eventId)
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

            await containerClient.CreateIfNotExistsAsync();

            string fileName =
                $"event-banners/{eventId}/{Guid.NewGuid()}{extension}";

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