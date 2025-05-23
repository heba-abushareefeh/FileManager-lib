using FileManager.Interfaces;
using FileManager.Services;
using FileManager.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace FileManager.Extensions
{
    public static class FileManagerServiceCollectionExtensions
    {
        public static IServiceCollection AddFileManager(this IServiceCollection services, IConfiguration configuration, string sectionName = "FileManager")
        {
            services.Configure<FileManagerOptions>(configuration.GetSection(sectionName));
            services.AddScoped<IFileManager, Services.FileManager>();
            // Enables access to HttpContext (e.g., for reading the request)
            services.AddHttpContextAccessor();
            return services;
        }

        public static IApplicationBuilder UseFileManagerStaticFiles(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices
                          .GetRequiredService<IOptions<FileManagerOptions>>()
                          .Value;
            var folder=options.RootFolderName;
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), folder);

            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                folderPath),
                RequestPath = $"/{folder}"
            });
        }

    }
}
