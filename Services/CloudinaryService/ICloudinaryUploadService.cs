using Entities.Cloudinary;
using Microsoft.AspNetCore.Http;

namespace Services.CloudinaryService
{
    public interface ICloudinaryUploadService
    {
        Task<CloudinaryUploadResult> AddPhoto(IFormFile file);
        Task<string> DeletePhoto(string publicId);
    }
}