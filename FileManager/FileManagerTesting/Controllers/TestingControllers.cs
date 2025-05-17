using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileManager.Interfaces;
using FileManager.Services;
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
                var fileName = await _file.UploadFileAsync(file,FileType.Video);
                var fileUrl = $"{Request.Scheme}://{Request.Host}/Uploads/Video/{fileName}";
                return Ok(new { url = fileUrl } );
            }
            catch (Exception e)
            {
                return StatusCode(500, "An Error Occurs: " + e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteFileTesting(string fileName)
        {
            try
            {
                var result=_file.DeleteFileByName(fileName,FileType.Video);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, "An Error Occurs: " + e.Message);
            }
        }



    }
}
