namespace BookSystem.Models
{
    public class Folder
    {
        public int Id { get; set; }

        public string FolderImg { get; set; }
        public string FolderName { get; set; }

        public List<Book> books { get; set; } = new List<Book>();
    }
}
