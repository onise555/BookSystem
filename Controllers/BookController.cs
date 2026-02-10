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
            var folder = await _data.Folders.Include(x => x.books)
                .FirstOrDefaultAsync(x => x.Id == req.FolderId);
            if (folder == null) return NotFound("Folder Not Found");

            var imgPath = await FileUploadHelper.UploadImg(req.BookImg, "book");

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

            // Image: თუ არ მოვიდა ფაილი -> ძველი დარჩეს
            var imgPath = book.BookImg;
            var uploaded = await FileUploadHelper.UploadImg(req.BookImg, "book");
            if (!string.IsNullOrWhiteSpace(uploaded))
                imgPath = uploaded;

            // Folder move optional
            if (req.FolderId.HasValue && req.FolderId.Value > 0)
            {
                // optional validation
                var folderExists = await _data.Folders.AnyAsync(f => f.Id == req.FolderId.Value);
                if (!folderExists) return BadRequest("Folder not found");

                book.FolderId = req.FolderId.Value;
            }

            // Title optional
            if (!string.IsNullOrWhiteSpace(req.Title))
                book.Title = req.Title.Trim();

            // bool? partial update
            if (req.IsRead.HasValue) book.IsRead = req.IsRead.Value;
            if (req.Liked.HasValue) book.Liked = req.Liked.Value;
            if (req.IsBought.HasValue) book.IsBought = req.IsBought.Value;

            book.BookImg = imgPath;

            await _data.SaveChangesAsync();

            var fullImgUrl = imgPath != null ? $"{Request.Scheme}://{Request.Host}{imgPath}" : null;

            return Ok(new
            {
                book.Id,
                book.Title,
                ImageUrl = fullImgUrl,
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
