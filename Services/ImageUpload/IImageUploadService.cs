using Entities;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Services.ImageUpload
{
    public interface IImageUploadService
    {
        Task<ServiceResponse> DeleteImage(string username, string publicId, StringBuilder logs);
        Task<ServiceResponse> SetProfilePicture(string username, string publicId, StringBuilder logs);
        Task<ServiceResponse> UploadImage(string username, IFormFile File, StringBuilder logs);
    }
}