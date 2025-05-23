# FileManager

A flexible .NET 8 library for managing file uploads, downloads, and deletions in ASP.NET Core applications. Designed to provide robust functionality such as file validation, size limits, compression, and multiple file handling.

## Features

- ✅ **File Upload**: Supports uploading single or multiple files with type validation.
- ✅ **File Deletion**: Delete files by name and type.
- ✅ **Download Files**: Retrieve files as byte arrays for downloading.
- ✅ **Validation**: Validates file extensions based on type (Image, Document, Video, Other).
- ✅ **Size Limit Control**: Configurable max file size and total upload limit.
- ✅ **Compression**: Compress individual files into ZIP archives.
- ✅ **Static File Serving**: Extension method to serve uploaded files via middleware.
- ✅ **Configuration Support**: Configure allowed extensions, root folder, and max file size via `appsettings.json`.
- ✅ **Dependency Injection Ready**: Built-in support for DI using `AddFileManager()` extension.

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package FileManager
```

## Configuration

Add the following section to your `appsettings.json`:

```json
"FileManager": {
  "RootFolderName": "Uploads",
  "MaxFileSizeBytes": 10485760,
  "AllowedImageExtensions": [".jpg", ".jpeg", ".png", ".gif"],
  "AllowedDocumentExtensions": [".pdf", ".docx", ".txt"],
  "AllowedVideoExtensions": [".mp4", ".avi", ".mov"]
}
```

## Usage

### 1. Register the File Service

In `Program.cs`:

```csharp
builder.Services.AddFileManager(builder.Configuration);
```

Optionally enable static file serving:

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/Uploads"
});
```

### 2. Inject and Use File Service

```csharp
public class FileController : ControllerBase
{
    private readonly IFileManager _fileManager;

    public FileController(IFileManager fileManager)
    {
        _fileManager = fileManager;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            var fileName = await _fileManager.UploadFileAsync(file, FileType.Image);
            var fileUrl = $"{Request.Scheme}://{Request.Host}/Uploads/Image/{fileName}";
            return Ok(new { Url = fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("download")]
    public IActionResult DownloadFile(string fileName)
    {
        try
        {
            var fileBytes = _fileManager.DownloadFileByName(fileName, FileType.Image);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error downloading file: {ex.Message}");
        }
    }

    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultipleFiles([FromForm] List<IFormFile> files, double? maxSizeInMB)
    {
        try
        {
            var result = await _fileManager.UploadMultipleFilesAsync(files, FileType.Document, maxSizeInMB);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error uploading files: {ex.Message}");
        }
    }

    [HttpDelete("delete")]
    public IActionResult DeleteFile(string fileName)
    {
        try
        {
            var result = _fileManager.DeleteFileByName(fileName, FileType.Video);
            return Ok(new { Success = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting file: {ex.Message}");
        }
    }

    [HttpPost("compress")]
    public async Task<IActionResult> CompressFile(IFormFile file)
    {
        try
        {
            var compressedBytes = await _fileManager.CompressFileToZipAsync(file);
            return File(compressedBytes, "application/zip", $"{Path.GetFileNameWithoutExtension(file.FileName)}.zip");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error compressing file: {ex.Message}");
        }
    }
}
```

## API Reference

### `FileType`

An enum representing supported file categories:

- `Image`
- `Document`
- `Video`
- `Other`

### `IFileManager`

| Method | Description |
|--------|-------------|
| `Task<string> UploadFileAsync(IFormFile file, FileType type = FileType.Other)` | Uploads a single file and returns the saved filename. |
| `Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, FileType type = FileType.Other, double? maxSizeInMB = null)` | Uploads multiple files with optional total size limit. |
| `bool DeleteFileByName(string fileName, FileType type = FileType.Other)` | Deletes a file by name and type. |
| `byte[] DownloadFileByName(string fileName, FileType type = FileType.Other)` | Returns the file content as bytes for download. |
| `Task<byte[]> CompressFileToZipAsync(IFormFile file)` | Compresses a file into a ZIP archive. |
| `bool IsValidExtensions(FileType type, string fileExtension, out List<string> allowedExtensions)` | Validates if the file extension is allowed for the given type. |
| `double GetTotalSizeInMB(List<IFormFile> files)` | Calculates total size of multiple files in MB. |

### `FileManagerOptions`

| Property | Type | Description |
|---------|------|-------------|
| `RootFolderName` | string | Root directory where files are stored. Default: `"Uploads"` |
| `MaxFileSizeBytes` | long | Max allowed size for a single file. Default: `10 * 1024 * 1024` (10 MB) |
| `AllowedImageExtensions` | List<string> | Allowed image file extensions. |
| `AllowedDocumentExtensions` | List<string> | Allowed document file extensions. |
| `AllowedVideoExtensions` | List<string> | Allowed video file extensions. |

## Authors

- Heba Abu Shareefeh
- Mohamamd Mrayyan

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Repository

[GitHub Repository](https://github.com/heba-abushareefeh/FileManager-lib)