namespace BookSystem.Requests
{
    public class UpdateFolderRequest
    {
        public IFormFile? FolderImg { get; set; }
        public  string? FolderName { get; set; }
    }
}
