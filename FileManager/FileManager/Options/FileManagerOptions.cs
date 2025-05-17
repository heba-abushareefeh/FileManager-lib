using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Options
{
    public class FileManagerOptions
    {
        //public string BaseUrl { get; set; } = string.Empty;
        public string RootFolderName { get; set; } = "Uploads";
        public List<string> AllowedImageExtensions { get; set; } = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };
        public List<string> AllowedDocumentExtensions { get; set; } = new List<string> { ".pdf", ".docx", ".txt" };
        public List<string> AllowedVideoExtensions { get; set; } = new List<string> { ".mp4", ".avi", ".mov" };
        public long MaxFileSizeBytes= 10 * 1024 * 1024;
    }

}
