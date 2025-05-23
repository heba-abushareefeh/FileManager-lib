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
using System.IO.Compression;
using Azure.Storage.Blobs;


namespace FileManager.Services
{

    internal class FileManager : IFileManager
    {
        private readonly FileManagerOptions _option;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public FileManager(IOptions<FileManagerOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            _option = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }
        private static readonly List<string> BlacklistedExtensions = new List<string>
        {
            ".exe", ".bat", ".sh", ".cmd", ".com", ".msi", ".vbs", ".js", ".scr", ".jar", ".ps1"
        };

        private bool IsValidExtension(string extension)
           => _option.AllowedExtensions.Contains(extension);

        private string GetDirectoryPath(string? subFolderName = null)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), _option.RootFolderName);
            return subFolderName != null ? Path.Combine(basePath, subFolderName) : basePath;
        }
        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty.");

            if (file.Length > _option.MaxSingleFileSizeBytes)
                throw new ArgumentException($"File size ({file.Length / (1024 * 1024)} MB) exceeds the allowed limit of {_option.MaxSingleFileSizeBytes / (1024 * 1024)} MB.");

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (BlacklistedExtensions.Contains(extension))
                throw new Exception("This file type is not allowed for security reasons.");

            if (!IsValidExtension(extension))
                throw new Exception($"Invalid file type. Allowed extensions: {string.Join(", ", _option.AllowedExtensions)}");
        }
        private string GenerateFileUrl(string fileName, string? subFolderName)
        {
            var request = _httpContextAccessor.HttpContext?.Request
                ?? throw new Exception("Unable to access HTTP context.");

            return subFolderName != null
                ? $"{request.Scheme}://{request.Host}/{_option.RootFolderName}/{subFolderName}/{fileName}"
                : $"{request.Scheme}://{request.Host}/{_option.RootFolderName}/{fileName}";
        }

        public double GetTotalSizeInMB(List<IFormFile> files)
           => Math.Round((files?.Sum(f => f.Length) ?? 0) / (1024.0 * 1024.0), 2);


        public async Task<string> UploadFileAsync(IFormFile file, string? subFolderName=null)
        {
            ValidateFile(file);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var directoryPath= GetDirectoryPath(subFolderName);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);


            string fullFilePath = Path.Combine(directoryPath, fileName);
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return GenerateFileUrl(fileName, subFolderName);
        }

        public bool DeleteFileByName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new Exception("Invalid File");

            var baseDirectory = GetDirectoryPath(); 

            var files = Directory.GetFiles(baseDirectory, fileName, SearchOption.AllDirectories);

            if (files.Length == 0)
                throw new FileNotFoundException("File not found: " + fileName);

            foreach (var filePath in files)
            {
                System.IO.File.Delete(filePath);
            }

            return true;
        }
        public byte[] DownloadFileByName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new Exception("Invalid File");

            var baseDirectory = GetDirectoryPath();

            var file = Directory.GetFiles(baseDirectory, fileName, SearchOption.AllDirectories).SingleOrDefault();

            if (file== null)
                throw new FileNotFoundException("File not found: " + fileName);

            return System.IO.File.ReadAllBytes(file);
        }

        public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, string? subFolderName=null )
        {
            if (files == null || files.Count == 0)
                throw new Exception("No files provided.");

            double totalSizeInMB = GetTotalSizeInMB(files);
            if (totalSizeInMB > _option.MaxTotalFilesSizeInMB)
                throw new ArgumentException($"Files size ({totalSizeInMB} MB) exceeds the allowed limit of {_option.MaxTotalFilesSizeInMB} MB.");

            var uploadedFiles = new List<string>();
            foreach (var file in files)
            {
                var path = await UploadFileAsync(file, subFolderName);
                uploadedFiles.Add(path);
            }

            return uploadedFiles;
        }

        public async Task<string> UploadCompressedFileAsync(IFormFile file, string? subFolderName = null)
        {
            ValidateFile(file);

            var zipFileName = Guid.NewGuid() + ".zip";
            var directoryPath = GetDirectoryPath(subFolderName);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var fullZipPath = Path.Combine(directoryPath, zipFileName);

            using (var zipToCreate = new FileStream(fullZipPath, FileMode.Create))
            using (var archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
            {
                var zipEntry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);
                using var entryStream = zipEntry.Open();
                using var inputStream = file.OpenReadStream();
                await inputStream.CopyToAsync(entryStream);
            }

            return GenerateFileUrl(zipFileName, subFolderName);
        }
        public async Task<List<string>> UploadMultipleCompressedFilesAsync(List<IFormFile> files, string? subFolderName = null)
        {
            if (files == null || files.Count == 0)
                throw new Exception("No files provided.");

            double totalSizeInMB = GetTotalSizeInMB(files);
            if (totalSizeInMB > _option.MaxTotalFilesSizeInMB)
                throw new ArgumentException($"Files size ({totalSizeInMB} MB) exceeds the allowed limit of {_option.MaxTotalFilesSizeInMB} MB.");

            var uploadedFiles = new List<string>();
            foreach (var file in files)
            {
                var path = await UploadCompressedFileAsync(file, subFolderName);
                uploadedFiles.Add(path);
            }

            return uploadedFiles;
        }
        


    }
}
