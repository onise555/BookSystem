using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace BookSystem.Services
{
    public class S3Service
    {
        private readonly IConfiguration _config;

        public S3Service(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            try
            {
                // Credentials
                var credentials = new BasicAWSCredentials(
                    _config["S3Config:AccessKey"],
                    _config["S3Config:SecretKey"]
                );

                // S3 Config - დაამატეთ RegionEndpoint
                var s3Config = new AmazonS3Config
                {
                    ServiceURL = _config["S3Config:ServiceUrl"],
                    AuthenticationRegion = _config["S3Config:Region"], // დაამატეთ ეს!
                    ForcePathStyle = true
                };

                using var client = new AmazonS3Client(credentials, s3Config);

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";

                using var stream = file.OpenReadStream();

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = fileName,
                    BucketName = _config["S3Config:BucketName"],
                    CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(client);
                await fileTransferUtility.UploadAsync(uploadRequest);

                // URL დაბრუნება
                return $"{_config["S3Config:ServiceUrl"]}/{_config["S3Config:BucketName"]}/{fileName}";
            }
            catch (Exception ex)
            {
                // ლოგირება
                Console.WriteLine($"S3 Upload Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}