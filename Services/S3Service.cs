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
            // მონაცემების წამოღება appsettings.json-დან
            var credentials = new BasicAWSCredentials(_config["S3Config:AccessKey"], _config["S3Config:SecretKey"]);
            var s3Config = new AmazonS3Config
            {
                ServiceURL = _config["S3Config:ServiceUrl"],
                ForcePathStyle = true
            };

            using var client = new AmazonS3Client(credentials, s3Config);

            // სურათს ვაძლევთ უნიკალურ სახელს, რომ სერვერზე ფაილები არ აირიოს
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = _config["S3Config:BucketName"],
                CannedACL = S3CannedACL.PublicRead // ეს ხაზი სურათს საჯაროს ხდის
            };

            var fileTransferUtility = new TransferUtility(client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            // ვაბრუნებთ სრულ ლინკს, რომელიც ბაზაში უნდა შეინახო
            return $"{_config["S3Config:ServiceUrl"]}/{_config["S3Config:BucketName"]}/{fileName}";
        }
    }
}
