namespace BookSystem.Dtos
{
    public class BookDtos
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string? BookImg { get; set; }

        public string? Description { get; set; }

        public string? Author { get; set; }

        public bool? IsRead { get; set; }
        public bool? Liked { get; set; }
        public bool? IsBought { get; set; }
    }
}
