using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace invex_api.Services
{
    public interface IStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string bucketName);
    }

    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;

        public S3StorageService(IConfiguration configuration)
        {
            var accessKey = configuration["YandexS3:AccessKey"];
            var secretKey = configuration["YandexS3:SecretKey"];
            var region = configuration["YandexS3:Region"] ?? "ru-central1";

            var config = new AmazonS3Config
            {
                ServiceURL = $"https://s3.{region}.amazonaws.com", // Fallback if Yandex URL not explicit
                AuthenticationRegion = region,
            };

            // Yandex Cloud S3 Endpoint override
            config.ServiceURL = "https://s3.yandexcloud.net";

            _s3Client = new AmazonS3Client(accessKey, secretKey, config);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string bucketName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = uniqueFileName,
                BucketName = bucketName,
                CannedACL = S3CannedACL.PublicRead,
                ContentType = file.ContentType,
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            // Construct public URL
            return $"https://{bucketName}.storage.yandexcloud.net/{uniqueFileName}";
        }
    }
}
