namespace BookSystem.Models
{
    public class BookDetail
    {
        public int Id { get; set; }


        public string? Description { get; set; }

        public string? Author { get; set; }

        public int BookId { get; set; } 

        public Book Book { get; set; }
    }
}
