using BookSystem.Models;

namespace BookSystem.Filters
{
    public class SearchByName
    {

        public int Id { get; set; } 
        public string FolderImg { get; set; }
        public string FolderName { get; set; }

        public string Title { get; set; }
        public string? BookImg { get; set; }

    }
}
