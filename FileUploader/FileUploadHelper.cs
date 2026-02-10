namespace BookSystem.FileUploader
{
    public class FileUploadHelper
    {
        public static async Task<string?> UploadImg(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            // Railway Volume root (env-ით მართვადი). ლოკალზე ჩავარდება wwwroot/uploads-ში.
            var uploadRoot = Environment.GetEnvironmentVariable("UPLOAD_ROOT");

            string physicalRoot;
            if (!string.IsNullOrWhiteSpace(uploadRoot))
            {
                // PROD: /data/uploads  (volume)
                physicalRoot = uploadRoot;
            }
            else
            {
                // LOCAL DEV fallback: wwwroot/uploads
                physicalRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            }

            // folder მაგალითად: "book"
            var targetDir = Path.Combine(physicalRoot, folder);
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(targetDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // DB-ში რასაც შეინახავ (public URL)
            return $"/uploads/{folder}/{fileName}";
        }
    }
}
