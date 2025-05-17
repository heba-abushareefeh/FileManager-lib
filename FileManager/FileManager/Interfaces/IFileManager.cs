using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FileManager.Services.FileManager;
using FileManager.Services;
namespace FileManager.Interfaces
{
    public interface IFileManager
    {
        public Task<string> UploadFileAsync(IFormFile file, FileType type = FileType.Other);

        public bool IsValidExtensions(FileType type, string fileExtensions, out List<string> allowedExtensions);
        public double GetTotalSizeInMB(List<IFormFile> files);
        public bool DeleteFileByName(string fileName, FileType type = FileType.Other);
        public byte[] DownloadFileByName(string fileName, FileType type = FileType.Other);
        public Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, FileType type = FileType.Other, double? maxSizeInMB=null);
        public Task<byte[]> CompressFileToZipAsync(IFormFile file);
    }
}

