using FileManager.Interfaces;
using FileManager.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static FileManager.Services.FileManager;

namespace FileManager.Services
{
    public class FileManager : IFileManager
    {

        private readonly FileManagerOptions _option;

        public FileManager(IOptions<FileManagerOptions> options)
        {
            _option = options.Value;
        }
        private static readonly List<string> BlacklistedExtensions = new List<string>
        {
            ".exe", ".bat", ".sh", ".cmd", ".com", ".msi", ".vbs", ".js", ".scr", ".jar", ".ps1"
        };


        public enum FileType
        {
            Image,
            Document,
            Video,
            Other
        }
        public bool IsValidExtensions(FileType type,string fileExtensions,out List<string> allowedExtensions)
        {
            if (type == FileType.Image)
            {
                allowedExtensions=_option.AllowedImageExtensions;
                return _option.AllowedImageExtensions.Contains(fileExtensions);
            }
            if (type == FileType.Document)
            {
                allowedExtensions = _option.AllowedDocumentExtensions;
                return _option.AllowedDocumentExtensions.Contains(fileExtensions);
            }
            if (type == FileType.Video)
            {
                allowedExtensions = _option.AllowedVideoExtensions;
                return _option.AllowedVideoExtensions.Contains(fileExtensions);
            }
            allowedExtensions= new List<string>();
            return true;
        }
        public async Task<string> UploadFileAsync(IFormFile file, FileType type = FileType.Other)
        {

            if (file == null || file.Length == 0)
                throw new Exception("File is empty.");

                if (file.Length > _option.MaxFileSizeBytes)
                    throw new ArgumentException($"File size ({file.Length / (1024 * 1024)} MB) exceeds the allowed limit of {_option.MaxFileSizeBytes / (1024 * 1024)} MB.");


            var extension = Path.GetExtension(file.FileName).ToLower();

            if (BlacklistedExtensions.Contains(extension))
                throw new Exception("This file type is not allowed for security reasons.");

            if (!IsValidExtensions(type, extension,out List<string> allowedExtensions))
                throw new Exception($"Invalid file type. Allowed extensions for {type}: {string.Join(", ", allowedExtensions)}");

            // هذا الباث اللي شغال فيه التطبيق، يعني مجلد البروسيس (exe or dll location)
            _option.BaseUrl = Directory.GetCurrentDirectory();

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var directoryPath=Path.Combine(_option.BaseUrl,_option.RootFolderName,type.ToString());
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string fullFilePath = Path.Combine(directoryPath, fileName);
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fullFilePath;


        }
    }
}
