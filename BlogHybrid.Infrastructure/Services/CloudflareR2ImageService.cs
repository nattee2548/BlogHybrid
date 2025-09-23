using Amazon.S3;
using Amazon.S3.Model;
using BlogHybrid.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace BlogHybrid.Infrastructure.Services
{
    public class CloudflareR2ImageService : IImageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CloudflareR2ImageService> _logger;
        private readonly string _bucketName;
        private readonly string _publicDomain;

        public CloudflareR2ImageService(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<CloudflareR2ImageService> logger)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _logger = logger;
            _bucketName = configuration["CloudflareR2:BucketName"]
                ?? throw new InvalidOperationException("CloudflareR2:BucketName not configured");
            _publicDomain = configuration["CloudflareR2:PublicDomain"]
                ?? throw new InvalidOperationException("CloudflareR2:PublicDomain not configured");
        }


        public async Task<string> UploadAsync(IFormFile file, string folder = "uploads")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException($"File type {extension} is not allowed");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size cannot exceed 5MB");

            try
            {
                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var key = $"{folder}/{fileName}";

                // อ่านไฟล์ทั้งหมดเข้า memory ก่อน
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Upload to R2
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = memoryStream,
                    ContentType = GetContentType(extension),
                    DisablePayloadSigning = true,  // สำคัญ - แก้ปัญหา payload signing
                    DisableDefaultChecksumValidation = true,
                    UseChunkEncoding = false       // สำคัญ - ปิด chunk encoding
                };

                var response = await _s3Client.PutObjectAsync(request);

                _logger.LogInformation("Successfully uploaded file {FileName} to R2 with key {Key}",
                    file.FileName, key);

                // Return the key (path) to store in database
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file {FileName} to R2", file.FileName);
                throw new InvalidOperationException("Failed to upload file to cloud storage", ex);
            }
        }
        public string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return "/images/placeholder.jpg"; // Default placeholder

            // If already a full URL, return as-is
            if (imagePath.StartsWith("http"))
                return imagePath;

            // Convert relative path to full R2 URL
            return $"{_publicDomain.TrimEnd('/')}/{imagePath.TrimStart('/')}";
        }

        public async Task<bool> DeleteAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            try
            {
                // Extract key from path (remove domain if present)
                var key = imagePath.StartsWith("http")
                    ? imagePath.Split('/').Skip(3).Aggregate((a, b) => $"{a}/{b}")
                    : imagePath.TrimStart('/');

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);

                _logger.LogInformation("Successfully deleted file with key {Key} from R2", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file {ImagePath} from R2", imagePath);
                return false;
            }
        }

        public bool Exists(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            try
            {
                var key = imagePath.StartsWith("http")
                    ? imagePath.Split('/').Skip(3).Aggregate((a, b) => $"{a}/{b}")
                    : imagePath.TrimStart('/');

                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                _s3Client.GetObjectMetadataAsync(request).Wait();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetUploadUrlAsync(string fileName, string folder = "uploads")
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var key = $"{folder}/{uniqueFileName}";

            // For direct upload, you might want to implement presigned URLs
            // For now, we'll return the key that will be used
            return key;
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
