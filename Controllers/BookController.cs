using BookSystem.Data;
using BookSystem.Dtos;
using BookSystem.FileUploader;
using BookSystem.Models;
using BookSystem.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {


        private readonly DataContext _data;

        public BookController(DataContext data)
        {
            _data = data;
        }
        [HttpPost("Add-Book-Folder")]
        public async Task<IActionResult> AddBook([FromForm] CreateBookRequest req)
        {
            string? imgPath = null;
            if (req.BookImg != null)
            {
                imgPath = await FileUploadHelper.UploadImg(req.BookImg, "book");

            }

            var folder = _data.Folders.Include(x => x.books).FirstOrDefault(x => x.Id == req.FolderId);
            if (folder == null) return NotFound();

            var book = new Book
            {
                Title = req.Title,
                BookImg = imgPath,
                IsRead = req.IsRead,
                Liked = req.Liked,
                IsBought = req.IsBought,
                FolderId = req.FolderId
            };

            folder.books.Add(book);
            await _data.SaveChangesAsync();

            var fullImgUrl = imgPath != null ? $"{Request.Scheme}://{Request.Host}{imgPath}" : null;

            return Ok(new
            {
                book.Id,
                book.Title,
                book.IsRead,
                book.Liked,
                book.IsBought,
                ImageUrl = fullImgUrl
            });
        }



        [HttpGet("Get-Book/{id}")]
        public ActionResult GetBook(int id)
        {
            var book = _data.books.Where(x => x.FolderId == id).
            Select(x => new BookDtos
            {
                Id = x.Id,
                Title = x.Title,
                IsRead = x.IsRead,
                BookImg = x.BookImg,
                IsBought = x.IsBought,
                Liked = x.Liked,
            }).ToList();

            return Ok(book);


        }
        [HttpGet("Get-Book-Bookid/{id}")]
        public ActionResult GetOnlyBook(int id)
        {
            var book =_data.books.Where(x=>x.Id==id).Select(x => new BookDtos { Id=x.Id, Title=x.Title, BookImg= x.BookImg, IsBought=x.IsBought, IsRead=x.IsRead, Liked= x.Liked }).FirstOrDefault();
            return Ok(book);
        }


        [HttpPut("Update/Book/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] UpdateBookRequest req)
        {
            var book = await _data.books.FirstOrDefaultAsync(x => x.Id == id);
            if (book == null) return NotFound("Book Not Found");

            var imgPath = book.BookImg;
            if (req.BookImg != null)
                imgPath = await FileUploadHelper.UploadImg(req.BookImg, "book");

           book.FolderId=req.FolderId;
            book.Title = req.Title;
            book.BookImg = imgPath;
            book.IsRead = req.IsRead;
            book.Liked = req.Liked;
            book.IsBought = req.IsBought;

            await _data.SaveChangesAsync();

            var fullImgUrl = imgPath != null ? $"{Request.Scheme}://{Request.Host}{imgPath}" : null;

            return Ok(new { book.Id, book.Title, ImageUrl = fullImgUrl, book.IsRead, book.Liked, book.IsBought });
        }



        [HttpDelete("Delete-Book/{id}")]

        public ActionResult DeleteBook(int id)
        {
            var book = _data.books.FirstOrDefault(x=>x.Id == id);  

            if (book == null)
            {
                return NotFound("Book Not Founded");
            }
            _data.books.Remove(book);   
            _data.SaveChanges();

            var deleteBookDtos = new DeleteBookDtos
            {
                Id = id,
            };

           return Ok(deleteBookDtos);   
        }




        [HttpGet("/test-file")]
        public IActionResult TestFile()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/uploads/movies/3dfcb672-d1df-4baa-97af-69a56b4deef7.jpg");

            return Ok(System.IO.File.Exists(path));
        }

    }
}   