namespace BookSystem.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string? BookImg { get; set; }

        public bool? IsRead { get; set; }
        public bool? Liked { get; set; }
        public bool? IsBought { get; set; }

        public int FolderId { get; set; }

        public Folder folder { get; set; }

        public BookDetail detail { get; set; }
    }
}
