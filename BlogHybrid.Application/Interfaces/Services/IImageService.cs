using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Interfaces.Services
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string folder = "uploads");
        string GetImageUrl(string? imagePath);
        Task<bool> DeleteAsync(string imagePath);
        bool Exists(string imagePath);
        Task<string> GetUploadUrlAsync(string fileName, string folder = "uploads");
    }
}
