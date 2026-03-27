using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

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
            if (file.ContentType.StartsWith("image/"))
            {
                return await UploadOptimizedImageAsync(file, folder);
            }

            var fileKey = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using var stream = file.OpenReadStream();
            return await ExecuteUpload(stream, fileKey, file.ContentType);
        }

        private async Task<string> UploadOptimizedImageAsync(IFormFile file, string folder)
        {
            var fileKey = $"{folder}/{Guid.NewGuid()}.webp";

            using var inputStream = file.OpenReadStream();
            // დაზუსტებული კლასის სახელი კონფლიქტის თავიდან ასაცილებლად
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(inputStream);
            using var outputStream = new MemoryStream();

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                // აუცილებლად SixLabors.ImageSharp.Size
                Size = new SixLabors.ImageSharp.Size(1200, 0)
            }));

            await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 75 });
            outputStream.Position = 0;

            return await ExecuteUpload(outputStream, fileKey, "image/webp");
        }

        private async Task<string> ExecuteUpload(Stream stream, string fileKey, string contentType)
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileKey,
                BucketName = _bucketName,
                ContentType = contentType
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