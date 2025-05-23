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
        public Task<string> UploadFileAsync(IFormFile file, string? subFolderName = null);

        public double GetTotalSizeInMB(List<IFormFile> files);
        public bool DeleteFileByName(string fileName);
        public byte[] DownloadFileByName(string fileName);
        public Task<List<string>>UploadMultipleFilesAsync(List<IFormFile> files, string? subFolderName= null);
        public Task<string> UploadCompressedFileAsync(IFormFile file, string? subFolderName = null);
        public Task<List<string>> UploadMultipleCompressedFilesAsync(List<IFormFile> files, string? subFolderName = null);
    }
}

