using BookSystem.Data;
using BookSystem.Dtos;
using BookSystem.FileUploader;
using BookSystem.Models;
using BookSystem.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        public ActionResult AddFolder(CreateFolderRequest req)
        {

            var folder = new Folder
            {
                FolderImg = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAR_FiWH3ZDD4awqEa9-ud522NBt089zlSAQ&s",
                FolderName = req.FolderName,

            };

            if(folder == null)
            {
                return BadRequest("this is bad ");
            }

            _data.Folders.Add(folder);  
            _data.SaveChanges();

            return Ok(folder);    

        }


        [HttpGet("Get-Folders")]
        public ActionResult GetFolders()
        {
            var folder =_data.Folders.Select(x=> new FolderDtos
            {
                Id=x.Id,
                FolderName = x.FolderName,
                FolderImg = x.FolderImg,
            });

            if (folder == null)
            {
                return NotFound("Folder Not Founded");
            }

            return Ok(folder);  

        }
        

        [HttpPut("Update/Folder{id}")]
        public async Task<IActionResult> UpdateFolder(int id, [FromForm] UpdateFolderRequest req)
        {
            var imgPath = await FileUploadHelper.UploadImg(
                req.FolderImg,
                "uploads/movies"
            );

            var folder = _data.Folders.FirstOrDefault(x => x.Id == id);

            if (folder == null)
            {
                return NotFound("Folder Not Founded");
            }

            folder.Id = id;
            folder.FolderName = req.FolderName;
            folder.FolderImg = imgPath;
            
            await _data.SaveChangesAsync();

            var fullImgUrl = $"{Request.Scheme}://{Request.Host}{imgPath}";

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
            if(folder == null)
            {
                return NotFound("Folder Not Founded");
            }

            _data.Folders.Remove(folder);
            _data.SaveChanges();

            var folderdeleteDto = new DeleteDtos
            {
                Id = id,
            };

            return Ok(folderdeleteDto); 
        }
    }

}
