using Microsoft.AspNetCore.Http;

namespace BookSystem.FileUploader
{
    public static class FileUploadHelper
    {
        // folder: "book" ან "folder"
        public static async Task<string?> UploadImg(IFormFile? file, string folder)
        {
            // Update-ზე: თუ ფაილი არ მოვიდა/ცარიელია -> ნუ შეცვლი (ვაბრუნებთ null)
            if (file == null || file.Length == 0)
                return null;

            // PROD: UPLOAD_ROOT=/data/uploads (volume)
            // LOCAL: wwwroot/uploads
            var uploadRoot = Environment.GetEnvironmentVariable("UPLOAD_ROOT");

            var physicalRoot = !string.IsNullOrWhiteSpace(uploadRoot)
                ? uploadRoot
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            var targetDir = Path.Combine(physicalRoot, folder);
            Directory.CreateDirectory(targetDir);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(targetDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // DB-ში ინახავ public URL-ს (Program.cs /uploads mapping ემსახურება ამ ფოლდერს)
            return $"/uploads/{folder}/{fileName}";
        }
    }
}
