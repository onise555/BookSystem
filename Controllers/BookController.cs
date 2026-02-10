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
        private readonly IConfiguration _config; // დაამატე ეს

        public BookController(DataContext data, IConfiguration config) // დაამატე აქაც
        {
            _data = data;
            _config = config; // მიანიჭე მნიშვნელობა
        }
        [HttpPost("Add-Book-Folder")]
        public async Task<IActionResult> AddBook([FromForm] CreateBookRequest req)
        {
            var folder = await _data.Folders.Include(x => x.books)
                .FirstOrDefaultAsync(x => x.Id == req.FolderId);
            if (folder == null) return NotFound("Folder Not Found");

            // გადავაწოდოთ _config მესამე პარამეტრად
            var imgPath = await FileUploadHelper.UploadImg(req.BookImg, "book", _config);

            var book = new Book
            {
                Title = req.Title,
                BookImg = imgPath, // აქ უკვე იქნება სრული URL (https://...)
                IsRead = req.IsRead,
                Liked = req.Liked,
                IsBought = req.IsBought,
                FolderId = req.FolderId
            };

            folder.books.Add(book);
            await _data.SaveChangesAsync();

            return Ok(new
            {
                book.Id,
                book.Title,
                book.IsRead,
                book.Liked,
                book.IsBought,
                ImageUrl = imgPath // პირდაპირ ვაბრუნებთ ატვირთულ მისამართს
            });
        }

        [HttpGet("Get-Book/{id}")]
        public IActionResult GetBook(int id)
        {
            var book = _data.books
                .Where(x => x.FolderId == id)
                .Select(x => new BookDtos
                {
                    Id = x.Id,
                    Title = x.Title,
                    IsRead = x.IsRead,
                    BookImg = x.BookImg,
                    IsBought = x.IsBought,
                    Liked = x.Liked,
                })
                .ToList();

            return Ok(book);
        }

        [HttpGet("Get-Book-Bookid/{id}")]
        public IActionResult GetOnlyBook(int id)
        {
            var book = _data.books
                .Where(x => x.Id == id)
                .Select(x => new BookDtos
                {
                    Id = x.Id,
                    Title = x.Title,
                    BookImg = x.BookImg,
                    IsBought = x.IsBought,
                    IsRead = x.IsRead,
                    Liked = x.Liked
                })
                .FirstOrDefault();

            return Ok(book);
        }

        [HttpPut("Update/Book/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] UpdateBookRequest req)
        {
            var book = await _data.books.FirstOrDefaultAsync(x => x.Id == id);
            if (book == null) return NotFound("Book Not Found");

            var imgPath = book.BookImg;
            // აქაც ვამატებთ _config-ს
            var uploaded = await FileUploadHelper.UploadImg(req.BookImg, "book", _config);

            if (!string.IsNullOrWhiteSpace(uploaded))
                imgPath = uploaded;

            // ... დანარჩენი ლოგიკა უცვლელია ...

            book.BookImg = imgPath;
            await _data.SaveChangesAsync();

            return Ok(new
            {
                book.Id,
                book.Title,
                ImageUrl = imgPath, // სრული URL უკვე გვაქვს
                book.FolderId,
                book.IsRead,
                book.Liked,
                book.IsBought
            });
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> MoveBook(int id, [FromBody] MoveBookRequest req)
        {
            if (req.FolderId <= 0) return BadRequest("FolderId is required");

            var book = await _data.books.FirstOrDefaultAsync(x => x.Id == id);
            if (book == null) return NotFound("Book Not Found");

            var folderExists = await _data.Folders.AnyAsync(f => f.Id == req.FolderId);
            if (!folderExists) return BadRequest("Folder not found");

            book.FolderId = req.FolderId;
            await _data.SaveChangesAsync();

            return Ok(new { book.Id, book.FolderId });
        }

        [HttpDelete("Delete-Book/{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = _data.books.FirstOrDefault(x => x.Id == id);
            if (book == null) return NotFound("Book Not Found");

            _data.books.Remove(book);
            _data.SaveChanges();

            return Ok(new { Id = id });
        }
    }



}
