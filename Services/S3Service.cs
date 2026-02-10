using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Runtime;

namespace BookSystem.Services
{
    public class S3Service
    {
        private readonly IConfiguration _config;

        public S3Service(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder = "book")
        {
            try
            {
                var accessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY");
                var secretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY");

                var credentials = new BasicAWSCredentials(accessKey, secretKey);

                var s3Config = new AmazonS3Config
                {
                    ServiceURL = _config["S3Config:ServiceUrl"],
                    AuthenticationRegion = _config["S3Config:Region"],
                    ForcePathStyle = true
                };

                using var client = new AmazonS3Client(credentials, s3Config);

                var fileName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                using var stream = file.OpenReadStream();

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = fileName,
                    BucketName = _config["S3Config:BucketName"],
                    ContentType = file.ContentType
                    // წაშალეთ CannedACL
                };

                var fileTransferUtility = new TransferUtility(client);
                await fileTransferUtility.UploadAsync(uploadRequest);

                // Signed URL გენერაცია (7 დღით)
                var presignedUrl = GetPresignedUrl(client, fileName, 7);

                return presignedUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"S3 Upload Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private string GetPresignedUrl(AmazonS3Client client, string key, int expirationDays)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _config["S3Config:BucketName"],
                Key = key,
                Expires = DateTime.UtcNow.AddDays(expirationDays)
            };

            return client.GetPreSignedURL(request);
        }
    }
}