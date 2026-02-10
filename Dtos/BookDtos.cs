namespace BookSystem.Dtos
{
    public class BookDtos
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string? BookImg { get; set; }

        public bool? IsRead { get; set; } = false;
        public bool? Liked { get; set; } = false;
        public bool? IsBought { get; set; }=false;  
    }
}
