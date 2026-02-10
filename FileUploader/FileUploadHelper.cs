using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BookSystem.FileUploader
{
    public static class FileUploadHelper
    {
        public static async Task<string?> UploadImg(IFormFile? file, string folder, IConfiguration config)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                // 1. მონაცემების წამოღება appsettings.json-დან
                var accessKey = config["S3Config:AccessKey"];
                var secretKey = config["S3Config:SecretKey"];
                var serviceUrl = config["S3Config:ServiceUrl"];
                var bucketName = config["S3Config:BucketName"];

                // 2. S3 კლიენტის კონფიგურაცია
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var s3Config = new AmazonS3Config
                {
                    ServiceURL = serviceUrl,
                    ForcePathStyle = true,
                    AuthenticationRegion = config["S3Config:Region"] ?? "auto"
                };

                using var client = new AmazonS3Client(credentials, s3Config);

                // 3. ფაილის სახელის მომზადება (folder/guid.ext)
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{folder}/{Guid.NewGuid()}{ext}";

                // 4. ატვირთვა
                using var stream = file.OpenReadStream();
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = fileName,
                    BucketName = bucketName,
                    // ეს ხაზი ხდის ფაილს საჯაროს
                    CannedACL = S3CannedACL.PublicRead,
                    // ეს ხაზი ეუბნება ბრაუზერს რომ ეს სურათია
                    ContentType = file.ContentType
                };

                var fileTransferUtility = new TransferUtility(client);
                await fileTransferUtility.UploadAsync(uploadRequest);

                // 5. ვაბრუნებთ მუდმივ საჯარო URL-ს
                return $"{serviceUrl}/{bucketName}/{fileName}";
            }
            catch (Exception ex)
            {
                // ლოგირება შეცდომის შემთხვევაში
                Console.WriteLine($"S3 Upload Error: {ex.Message}");
                return null;
            }
        }
    }
}