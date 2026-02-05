using BookSystem.Data;
using BookSystem.Filters;
using BookSystem.Models;
using BookSystem.Data;
using BookSystem.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiltersController : ControllerBase
    {

        private readonly DataContext _data;
        public FiltersController(DataContext data)
        {
            _data = data;
        }

        [HttpGet("Search-By/{name}")]
        public ActionResult SearchByName(string name)
        {

            var sear = _data.books.Include(x => x.folder).Select(x => new SearchByName
            {
                Id = x.Id,
                FolderImg = x.folder.FolderImg,
                FolderName = x.folder.FolderName,
                Title = x.Title,
                BookImg = x.BookImg

            }).Where(x => x.Title.Contains(name));


            return Ok(sear);

        }

    }
}
