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
        private readonly IConfiguration _config; // დავამატოთ კონფიგურაცია

        public FolderController(DataContext data, IConfiguration config)
        {
            _data = data;
            _config = config;
        }

        // AddFolder მეთოდში სურათი არ გაქვს, ამიტომ აქ არაფერი იცვლება
        [HttpPost("Add-Folder")]
        public IActionResult AddFolder([FromBody] CreateFolderRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.FolderName))
                return BadRequest("FolderName required");

            var name = req.FolderName.Trim();
            var exists = _data.Folders.Any(f => f.FolderName == name);
            if (exists) return BadRequest("Folder already exists");

            var folder = new Folder
            {
                FolderName = name,
                FolderImg = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAR_FiWH3ZDD4awqEa9-ud522NBt089zlSAQ&s"
            };

            _data.Folders.Add(folder);
            _data.SaveChanges();

            return Ok(folder);
        }


        [HttpGet("Get-Folders")]
        public IActionResult GetFolders()
        {
            var folders = _data.Folders
                .Select(x => new FolderDtos
                {
                    Id = x.Id,
                    FolderName = x.FolderName,
                    FolderImg = x.FolderImg
                })
                .OrderBy(x => x.Id)
                .ToList();

            return Ok(folders);
        }

        [HttpPut("Update/Folder/{id}")]
        public async Task<IActionResult> UpdateFolder(int id, [FromForm] UpdateFolderRequest req)
        {
            var folder = await _data.Folders.FirstOrDefaultAsync(x => x.Id == id);
            if (folder == null) return NotFound("Folder Not Found");

            var imgUrl = folder.FolderImg;

            // ვამატებთ _config-ს მესამე პარამეტრად
            var uploaded = await FileUploadHelper.UploadImg(req.FolderImg, "folder", _config);

            if (!string.IsNullOrWhiteSpace(uploaded))
                imgUrl = uploaded; // აქ უკვე იქნება Bucket-ის სრული ლინკი

            if (!string.IsNullOrWhiteSpace(req.FolderName))
                folder.FolderName = req.FolderName.Trim();

            folder.FolderImg = imgUrl;

            await _data.SaveChangesAsync();

            // აღარ გვჭირდება Request.Scheme და Request.Host-ით აწყობა
            return Ok(new
            {
                folder.Id,
                folder.FolderName,
                ImageUrl = imgUrl
            });
        }




        [HttpDelete("Delete-Folder/{id}")]
        public IActionResult DeleteFolder(int id)
        {
            var folder = _data.Folders.FirstOrDefault(x => x.Id == id);
            if (folder == null) return NotFound("Folder Not Found");

            _data.Folders.Remove(folder);
            _data.SaveChanges();

            return Ok(new { Id = id });
        }
    }


}

