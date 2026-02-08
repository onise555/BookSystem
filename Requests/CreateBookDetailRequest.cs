namespace BookSystem.Requests
{
    public class CreateBookDetailRequest
    {
        public string? Description { get; set; }

        public string? Author { get; set; }

        public int BookId { get; set; }
    }
}
