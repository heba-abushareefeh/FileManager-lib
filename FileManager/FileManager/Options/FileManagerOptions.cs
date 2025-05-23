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
        public List<string> AllowedExtensions { get; set; } = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".txt" , ".mp4", ".avi", ".mov" };
       
        public long MaxSingleFileSizeBytes { get; set; } = 10 * 1024 * 1024;

        public long MaxTotalFilesSizeInMB { get; set; } = 50 ;

        // إضافات السحابة
        public string? AzureBlobConnectionString { get; set; }
        public string? AzureBlobContainerName { get; set; }

    }

}
