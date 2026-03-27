using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace BookSystem.FileUploader
{
    public static class FileUploadHelper
    {
        public static async Task<string?> UploadImg(IFormFile? file, string folder, IConfiguration config)
        {
            if (file == null || file.Length == 0) return null;

            // S3 კონფიგურაცია
            var accessKey = config["S3Config:AccessKey"];
            var secretKey = config["S3Config:SecretKey"];
            var serviceUrl = config["S3Config:ServiceUrl"];
            var bucketName = config["S3Config:BucketName"];

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var s3Config = new AmazonS3Config { ServiceURL = serviceUrl, ForcePathStyle = true };
            using var client = new AmazonS3Client(credentials, s3Config);

            var fileKey = $"{folder}/{Guid.NewGuid()}.webp";

            try
            {
                using var inputStream = file.OpenReadStream();
                // ფოტოს ჩატვირთვა
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(inputStream);
                using var outputStream = new MemoryStream();

                // ოპტიმიზაცია: ზომის შეცვლა (მაქსიმუმ 1200px სიგანე)
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new SixLabors.ImageSharp.Size(1200, 0)
                }));

                // შენახვა WebP ფორმატში (75% ხარისხი საუკეთესო ბალანსია)
                await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 75 });
                outputStream.Position = 0;

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = outputStream,
                    Key = fileKey,
                    BucketName = bucketName,
                    ContentType = "image/webp"
                };

                var transferUtility = new TransferUtility(client);
                await transferUtility.UploadAsync(uploadRequest);

                var urlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = fileKey,
                    Expires = DateTime.UtcNow.AddYears(10)
                };

                return client.GetPreSignedURL(urlRequest);
            }
            catch (Exception ex)
            {
                // აქ შეგიძლია ლოგირება დაამატო საჭიროებისამებრ
                return null;
            }
        }
    }
}