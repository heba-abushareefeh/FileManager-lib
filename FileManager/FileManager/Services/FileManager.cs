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
using System.IO.Compression;


namespace FileManager.Services
{

    public enum FileType
    {
        Image,
        Document,
        Video,
        Other
    }

    internal class FileManager : IFileManager
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
        internal string GetDirectoryPath(FileType type)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), _option.RootFolderName, type.ToString());
        }
        public double GetTotalSizeInMB(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return 0;

            long totalBytes = files.Sum(f => f.Length);
            double totalMB = Math.Round((double)totalBytes / (1024 * 1024), 2); 
            return totalMB;
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
            //var BaseUrl = Directory.GetCurrentDirectory();

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            //var directoryPath=Path.Combine(BaseUrl,_option.RootFolderName,type.ToString());
            var directoryPath=GetDirectoryPath(type);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string fullFilePath = Path.Combine(directoryPath, fileName);
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;


        }

        public bool DeleteFileByName(string fileName, FileType type = FileType.Other)
        {
            if (string.IsNullOrEmpty(fileName)) throw new Exception("Invalid File");

            var directoryPath = GetDirectoryPath(type);
            var fullFilePath = Path.Combine(directoryPath, fileName);

            if (!System.IO.File.Exists(fullFilePath))
                throw new FileNotFoundException("File not found at path: " + fullFilePath);

            System.IO.File.Delete(fullFilePath);
            return true;
        }
        public byte[] DownloadFileByName(string fileName, FileType type = FileType.Other)
        {
            var path = Path.Combine(GetDirectoryPath(type), fileName);

            if (!System.IO.File.Exists(path))
                throw new FileNotFoundException();

            return System.IO.File.ReadAllBytes(path);
        }

        public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, FileType type = FileType.Other, double? maxSizeInMB=null) 
        {
            if (files == null || files.Count == 0)
                throw new Exception("No files provided.");

           
            double defaultMaxSizeInMB = Math.Round((double)(files.Count * _option.MaxFileSizeBytes) / (1024 * 1024), 2);
            maxSizeInMB = maxSizeInMB ?? defaultMaxSizeInMB;

            double totalSizeInMB = GetTotalSizeInMB(files);
            if (totalSizeInMB > maxSizeInMB)
                throw new ArgumentException($"Files size ({totalSizeInMB} MB) exceeds the allowed limit of {maxSizeInMB} MB.");

            var uploadedFiles = new List<string>();
            foreach (var file in files)
            {
                var path = await UploadFileAsync(file, type);
                uploadedFiles.Add(path);
            }

            return uploadedFiles;
        }

        public async Task<byte[]> CompressFileToZipAsync(IFormFile file)
        {
            using var outputStream = new MemoryStream();
            using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
            {
                var zipEntry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                using var entryStream = zipEntry.Open();
                using var inputStream = file.OpenReadStream();
                await inputStream.CopyToAsync(entryStream);
            }
            return outputStream.ToArray();
        }


    }
}
