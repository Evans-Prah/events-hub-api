using API.Extensions;
using Entities;
using Entities.Payload;
using Microsoft.AspNetCore.Mvc;
using Services.FileLogger;
using Services.ImageUpload;
using System.Text;

namespace API.Controllers
{
    [Route("api/image")]
    [ApiController]
    public class ImageUploadController : ControllerBase
    {
        private readonly IImageUploadService _imageUpload;
        private readonly IFileLogger _logger;

        public ImageUploadController(IImageUploadService imageUpload, IFileLogger logger)
        {
            _imageUpload = imageUpload;
            _logger = logger;
        }

        [HttpPost("[action]")]
        public async Task<ApiResponse> UploadImage([FromForm] IFormFile File)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            if (!ModelState.IsValid) return new ApiResponse { Success = false, ResponseMessage = "Invalid payload sent, check and try again" };

            try
            {
                var currentUser = SessionHelper.GetCurrentUser(HttpContext);
                if (currentUser == null) return new ApiResponse { Success = false, ResponseMessage = "Unauthorized request." };

                var process = await _imageUpload.UploadImage(currentUser.Username, File, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage, Data = process.Data };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while uploading image, try again later." };
            }
        }
        
        [HttpPost("[action]/{publicId}")]
        public async Task<ApiResponse> DeleteImage(string publicId)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            if (!ModelState.IsValid) return new ApiResponse { Success = false, ResponseMessage = "Invalid payload sent, check and try again" };

            try
            {
                var currentUser = SessionHelper.GetCurrentUser(HttpContext);
                if (currentUser == null) return new ApiResponse { Success = false, ResponseMessage = "Unauthorized request." };

                var process = await _imageUpload.DeleteImage(currentUser.Username, publicId, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while deleting image, try again later." };
            }
        }
        
        [HttpPost("[action]/{publicId}")]
        public async Task<ApiResponse> SetProfilePicture(string publicId)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            if (!ModelState.IsValid) return new ApiResponse { Success = false, ResponseMessage = "Invalid payload sent, check and try again" };

            try
            {
                var currentUser = SessionHelper.GetCurrentUser(HttpContext);
                if (currentUser == null) return new ApiResponse { Success = false, ResponseMessage = "Unauthorized request." };

                var process = await _imageUpload.SetProfilePicture(currentUser.Username, publicId, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while setting profile picture, try again later." };
            }
        }
    }
}
