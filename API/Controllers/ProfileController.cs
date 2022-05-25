using API.Extensions;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services.FileLogger;
using Services.ProfileService;

namespace API.Controllers
{
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IFileLogger _logger;

        public ProfileController(IProfileService profileService, IFileLogger logger)
        {
            _profileService = profileService;
            _logger = logger;
        }


        [HttpGet("[action]/{username}")]
        public async Task<ApiResponse> GetUserProfile(string username)
        {
            try
            {
                var currentUser = SessionHelper.GetCurrentUser(HttpContext);
                if (currentUser == null) return new ApiResponse { Success = false, ResponseMessage = "Unauthorized request." };

                var request = await _profileService.GetUserProfile(username);

                if (!request.Successful) return new ApiResponse { Success = false, ResponseMessage = request.ResponseMessage};

                return new ApiResponse { Success = true, ResponseMessage = request.ResponseMessage, Data = request.Data };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while fetching user profile, try again later." };
            }
        }
    }
}
