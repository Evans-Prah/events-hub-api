using API.Extensions;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services.FileLogger;
using Services.FollowService;
using System.Text;

namespace API.Controllers
{
    [Route("api/follow")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IFollowingService _followingService;
        private readonly IFileLogger _logger;

        public FollowController(IFollowingService followingService, IFileLogger logger)
        {
            _followingService = followingService;
            _logger = logger;
        }

        [HttpPost("[action]/{username}")]
        public async Task<ApiResponse> FollowOrUnfollow(string username)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            try
            {
                var currentUser = SessionHelper.GetCurrentUser(HttpContext);
                if (currentUser == null) return new ApiResponse { Success = false, ResponseMessage = "Unauthorized request." };

                var process = await _followingService.FollowOrUnfollowUser(currentUser.Username, username, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while processing the request, try again later." };
            }
        }

        [HttpGet("[action]/{username}")]
        public async Task<ApiResponse> GetFollowings(string username)
        {
            try
            {
                var followings = await _followingService.GetUserFollowings(username);

                if (followings == null || !followings.Any()) return new ApiResponse { Success = false, ResponseMessage = $"{username} is not following anyone" };

                return new ApiResponse { Success = true, ResponseMessage = $"{username} followings fetched successfully", Data = followings };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while fetching user followings, try again later." };
            }
        }
        
        [HttpGet("[action]/{username}")]
        public async Task<ApiResponse> GetFollowers(string username)
        {
            try
            {
                var followers = await _followingService.GetUserFollowers(username);

                if (followers == null || !followers.Any()) return new ApiResponse { Success = false, ResponseMessage = $"{username} has no followers" };

                return new ApiResponse { Success = true, ResponseMessage = $"{username} followers fetched successfully", Data = followers };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while fetching user followers, try again later." };
            }
        }
    }
}
