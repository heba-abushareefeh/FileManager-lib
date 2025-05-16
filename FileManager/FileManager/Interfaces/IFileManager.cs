using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FileManager.Services.FileManager;

namespace FileManager.Interfaces
{
    public interface IFileManager
    {
        public Task<string> UploadFileAsync(IFormFile file, FileType type = FileType.Other);
    }
}

