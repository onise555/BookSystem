using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Amazon.S3.Transfer;

namespace BookSystem.FileUploader
{
    public static class FileUploadHelper
    {
        public static async Task<string?> UploadImg(IFormFile? file, string folder, IConfiguration config)
        {
            if (file == null || file.Length == 0) return null;

            var accessKey = config["S3Config:AccessKey"];
            var secretKey = config["S3Config:SecretKey"];
            var serviceUrl = config["S3Config:ServiceUrl"];
            var bucketName = config["S3Config:BucketName"];

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var s3Config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(credentials, s3Config);
            var fileKey = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // 1. ატვირთვა
            using var stream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileKey,
                BucketName = bucketName,
                ContentType = file.ContentType
            };

            var transferUtility = new TransferUtility(client);
            await transferUtility.UploadAsync(uploadRequest);

            // 2. დროებითი (Presigned) ლინკის გენერაცია - ეს იმუშავებს პრივატულ ბაქეთზეც!
            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.AddYears(10) // ლინკი იმუშავებს 7 დღე
            };

            return client.GetPreSignedURL(urlRequest);
        }
    }
}