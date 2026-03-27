using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Runtime;

namespace BookSystem.Services
{
    public class S3Service
    {
        private readonly IConfiguration _config;
        private readonly AmazonS3Client _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration config)
        {
            _config = config;
            var accessKey = _config["S3Config:AccessKey"];
            var secretKey = _config["S3Config:SecretKey"];
            _bucketName = _config["S3Config:BucketName"];

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var s3Config = new AmazonS3Config
            {
                ServiceURL = _config["S3Config:ServiceUrl"],
                AuthenticationRegion = _config["S3Config:Region"],
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(credentials, s3Config);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder = "book")
        {
            // უბრალოდ ვაკოპირებთ ფაილს, ყოველგვარი დამუშავების გარეშე
            var fileKey = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using var stream = file.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileKey,
                BucketName = _bucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return GeneratePresignedUrl(fileKey);
        }

        public string GeneratePresignedUrl(string fileKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}