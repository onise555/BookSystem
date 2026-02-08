using BookSystem.Data;
using BookSystem.Dtos;
using BookSystem.Models;
using BookSystem.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookDetailController : ControllerBase
    {
        private readonly DataContext _data;

        public  BookDetailController(DataContext data)
        {
            _data = data;   
        }

        [HttpPost("Add-Book-Detail")]
        public ActionResult AddBookDetail(CreateBookDetailRequest req)
        {
            var detail = new BookDetail
            {
                Description = req.Description,
                Author = req.Author,
                BookId = req.BookId,
            };

            _data.details.Add(detail);  
            _data.SaveChanges();    

            return Ok(detail);  
        }

        [HttpGet("Get-Book-Detail/{id}")]

        public ActionResult GetBookDetail(int id)
        {
            var detail =_data.details.Where(x=>x.BookId==id).Select
                (x=> new GetDetailDtos
                {
                    Id =x.Id,
                    Author = x.Author,  
                    Description = x.Description,    
                }).FirstOrDefault();    

            return Ok(detail);  
        }
    }
}
