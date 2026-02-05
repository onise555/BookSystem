namespace BookSystem.FileUploader
{
    public class FileUploadHelper
    {
        public static async Task<string> UploadImg(IFormFile file, string Folder)
        {
            if (file == null || file.Length == 0)
                return null;

            var rootpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Folder);

            if (!Directory.Exists(rootpath))
                Directory.CreateDirectory(rootpath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(rootpath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{Folder}/{fileName}";

        }
    }

}
