using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileManager.Interfaces;
namespace FileManagerTesting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestingControllers : ControllerBase
    {

        private readonly IFileManager _file;

        public TestingControllers(IFileManager file)
        {
            _file = file;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFileTesting(IFormFile file)
        {
            try
            {
                var savedFilePath = await _file.UploadFileAsync(file, FileManager.Services.FileManager.FileType.Video);

                var fileName = Path.GetFileName(savedFilePath);
                var fileUrl = $"{Request.Scheme}://{Request.Host}/Uploads/Video/{fileName}";
               

                return Ok(new { url = fileUrl } );
            }
            catch (Exception e)
            {
                return StatusCode(500, "An Error Occurs: " + e.Message);
            }
        }


    }
}
