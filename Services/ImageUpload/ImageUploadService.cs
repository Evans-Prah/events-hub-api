using DBHelper;
using Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Services.CloudinaryService;
using System.Text;

namespace Services.ImageUpload
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IPostgresHelper _postgresHelper;
        private readonly ICloudinaryUploadService _cloudinaryUpload;

        public ImageUploadService(IPostgresHelper postgresHelper, ICloudinaryUploadService cloudinaryUpload)
        {
            _postgresHelper = postgresHelper;
            _cloudinaryUpload = cloudinaryUpload;
        }

        public async Task<ServiceResponse> UploadImage(string username, IFormFile File, StringBuilder logs)
        {
            logs.AppendLine("-- UploadImage");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, File })}");

            var upload = _cloudinaryUpload.AddPhoto(File);
            if (upload == null) return new ServiceResponse { Successful = false, ResponseMessage = "Upload failed" };

            var response = await _postgresHelper.UploadImage(username, upload.Result.PublicId, upload.Result.Url);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(response)}");

            if (!string.IsNullOrWhiteSpace(response.ResponseMessage)) return new ServiceResponse { Successful = false, ResponseMessage = response.ResponseMessage };

            return new ServiceResponse
            {
                Successful = true,
                ResponseMessage = "Image upload successful",
                Data = new ImageUploadDbResponse
                {
                    PublicId = response.PublicId,
                    Url = response.Url,
                    IsMainPhoto = response.IsMainPhoto
                }
            };
        }

        public async Task<ServiceResponse> DeleteImage(string username, string publicId, StringBuilder logs)
        {
            logs.AppendLine("-- DeleteImage");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, publicId })}");

            if (string.IsNullOrWhiteSpace(publicId)) return new ServiceResponse { Successful = false, ResponseMessage = "Photo Id is required" };

            var deleteImage = await _cloudinaryUpload.DeletePhoto(publicId);
            if (deleteImage == null) return new ServiceResponse { Successful = false, ResponseMessage = "Image deletion from cloudinary failed" };

            var response = await _postgresHelper.DeleteImage(username, publicId);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(response)}");

            if (!string.IsNullOrWhiteSpace(response)) return new ServiceResponse { Successful = false, ResponseMessage = response };

            return new ServiceResponse { Successful = true, ResponseMessage = "Image deleted successfully" };
        }
        
        public async Task<ServiceResponse> SetProfilePicture(string username, string publicId, StringBuilder logs)
        {
            logs.AppendLine("-- SetProfilePicture");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, publicId })}");

            if (string.IsNullOrWhiteSpace(publicId)) return new ServiceResponse { Successful = false, ResponseMessage = "Photo Id is required" };

            var response = await _postgresHelper.SetProfilePicture(username, publicId);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(response)}");

            if (!string.IsNullOrWhiteSpace(response)) return new ServiceResponse { Successful = false, ResponseMessage = response };

            return new ServiceResponse { Successful = true, ResponseMessage = "Profile picture set successfully" };
        }
    }
}
