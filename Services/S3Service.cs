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

                // მონაცემების წამოღება კონფიგურაციიდან
                var accessKey = _config["S3Config:AccessKey"];
                var secretKey = _config["S3Config:SecretKey"];
                _bucketName = _config["S3Config:BucketName"];

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var s3Config = new AmazonS3Config
                {
                    ServiceURL = _config["S3Config:ServiceUrl"],
                    AuthenticationRegion = _config["S3Config:Region"],
                    ForcePathStyle = true // აუცილებელია Railway/MinIO-სთვის
                };

                _s3Client = new AmazonS3Client(credentials, s3Config);
            }

            public async Task<string> UploadFileAsync(IFormFile file, string folder = "book")
            {
                // 1. ფაილის უნიკალური სახელის შექმნა
                var fileKey = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                using var stream = file.OpenReadStream();

                // 2. ატვირთვის მოთხოვნა
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = fileKey,
                    BucketName = _bucketName,
                    ContentType = file.ContentType
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);

                // 3. "საშვიანი" ლინკის გენერაცია (ვადა: 7 დღე)
                return GeneratePresignedUrl(fileKey);
            }

            public string GeneratePresignedUrl(string fileKey)
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey,
                    Expires = DateTime.UtcNow.AddDays(7) // ლინკი იმუშავებს 7 დღე
                };

                return _s3Client.GetPreSignedURL(request);
            }
        }
    }