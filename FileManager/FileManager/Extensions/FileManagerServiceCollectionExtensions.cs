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

namespace FileManager.Extensions
{
    public static class FileManagerServiceCollectionExtensions
    {
        public static IServiceCollection AddFileManager(this IServiceCollection services, IConfiguration configuration, string sectionName = "FileManager")
        {
            services.Configure<FileManagerOptions>(configuration.GetSection(sectionName));
            services.AddScoped<IFileManager, Services.FileManager>();
            return services;
        }

    }
}
