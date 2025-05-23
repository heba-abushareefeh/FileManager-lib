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
                //var type = FileType.Image;
                var fileUrl = await _file.UploadFileAsync(file);
                //Console.WriteLine(FileType.Image.ToString());
                //var fileUrl = $"{Request.Scheme}://{Request.Host}/Uploads/{type}/{fileName}";
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
                var result = _file.DeleteFileByName(fileName);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, "An Error Occurs: " + e.Message);
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> DownloadFileTesting(string fileName)
        {
            try
            {
                var fileBytes = _file.DownloadFileByName(fileName);
                return File(fileBytes, "application/octet-stream", fileName);

            }
            catch (Exception e)
            {
                return StatusCode(500, "An Error Occurs: " + e.Message);
            }
        }
        [HttpPost("UploadMultiple")]
        public async Task<IActionResult> UploadMultipleFiles([FromForm] List<IFormFile> files)
        {
            try
            {
                var result = await _file.UploadMultipleFilesAsync(files,"Image");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadCompressedFileTesting(IFormFile file)
        {
            try
            {
                //var type = FileType.Image;
                var fileUrl = await _file.UploadCompressedFileAsync(file);
                //Console.WriteLine(FileType.Image.ToString());
                //var fileUrl = $"{Request.Scheme}://{Request.Host}/Uploads/{type}/{fileName}";
                return Ok(new { url = fileUrl });
            }
            catch (Exception e)
            {
                return StatusCode(500, "An Error Occurs: " + e.Message);
            }




        }
    }
}
