using BookSystem.Data;
using BookSystem.Dtos;
using BookSystem.FileUploader;
using BookSystem.Models;
using BookSystem.Requests;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPut("Update/Folder{id}")]
        public async Task<IActionResult> UpdateFolder(int id, [FromForm] UpdateFolderRequest req)
        {
            var folder = _data.Folders.FirstOrDefault(x => x.Id == id);
            if (folder == null) return NotFound("Folder Not Found");

            // ✅ თუ ახალი სურათი არ მოვიდა, ძველი დატოვე
            var imgPath = folder.FolderImg;

            if (req.FolderImg != null)
            {
                // ✅ ახალი helper-ს სტილი: მხოლოდ subfolder
                imgPath = await FileUploadHelper.UploadImg(req.FolderImg, "folder");
            }

            if (!string.IsNullOrWhiteSpace(req.FolderName))
                folder.FolderName = req.FolderName.Trim();

            folder.FolderImg = imgPath;

            await _data.SaveChangesAsync();

            var fullImgUrl = imgPath != null ? $"{Request.Scheme}://{Request.Host}{imgPath}" : null;

            return Ok(new
            {
                folder.Id,
                folder.FolderName,
                ImageUrl = fullImgUrl
            });
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
