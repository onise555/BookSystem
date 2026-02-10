namespace BookSystem.Requests
{
    public class UpdateBookRequest
    {

        public string Title { get; set; }
        public IFormFile? BookImg { get; set; }

        public bool? IsRead { get; set; }
        public bool? Liked { get; set; }
        public bool? IsBought { get; set; }
        public int? FolderId { get; set; }
    }
}
