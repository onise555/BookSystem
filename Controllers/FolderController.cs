using BookSystem.Data;
using BookSystem.Dtos;
using BookSystem.FileUploader;
using BookSystem.Models;
using BookSystem.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly DataContext _data;

        public FolderController(DataContext data)
        {
            _data = data;
        }

        [HttpPost("Add-Folder")]
        public ActionResult AddFolder([FromBody] CreateFolderRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.FolderName))
                return BadRequest("FolderName required");

            var exists = _data.Folders.Any(f => f.FolderName == req.FolderName.Trim());
            if (exists)
                return BadRequest("Folder already exists");

            var folder = new Folder
            {
                FolderName = req.FolderName.Trim(),

                // თუ გინდა default image (არა upload)
                FolderImg = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAR_FiWH3ZDD4awqEa9-ud522NBt089zlSAQ&s"
            };

            _data.Folders.Add(folder);
            _data.SaveChanges();

            return Ok(folder);
        }

        [HttpGet("Get-Folders")]
        public ActionResult GetFolders()
        {
            var folders = _data.Folders
                .Select(x => new FolderDtos
                {
                    Id = x.Id,
                    FolderName = x.FolderName,
                    FolderImg = x.FolderImg
                })
                .OrderBy(x => x.Id)
                .ToList(); // ✅ materialize

            return Ok(folders);
        }



        [HttpPut("Update/Book/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] UpdateBookRequest req)
        {
            var book = await _data.books.FirstOrDefaultAsync(x => x.Id == id);
            if (book == null) return NotFound("Book Not Found");


            var imgPath = book.BookImg;
            if (req.BookImg != null && req.BookImg.Length > 0)
            {
                imgPath = await FileUploadHelper.UploadImg(req.BookImg, "book");
            }

            
            if (req.FolderId.HasValue && req.FolderId.Value > 0)
            {
               
                var folderExists = await _data.Folders.AnyAsync(f => f.Id == req.FolderId.Value);
                if (!folderExists) return BadRequest("Folder not found");

                book.FolderId = req.FolderId.Value;
            }



            book.BookImg = imgPath;

            await _data.SaveChangesAsync();

            var fullImgUrl = imgPath != null
                ? $"{Request.Scheme}://{Request.Host}{imgPath}"
                : null;

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
    

        [HttpDelete("Delete-Folder/{id}")]
        public ActionResult DeleteFolder(int id)
        {
            var folder = _data.Folders.FirstOrDefault(x => x.Id == id);
            if (folder == null) return NotFound("Folder Not Found");

            _data.Folders.Remove(folder);
            _data.SaveChanges();

            return Ok(new DeleteDtos { Id = id });
        }




    }
}
