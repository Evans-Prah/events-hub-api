using API.Extensions;
using Entities;
using Entities.Payload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.FileLogger;
using Services.UserAccount;
using System.Text;

namespace API.Controllers
{
    [AllowAnonymous]
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserAccountService _accountService;
        private readonly IFileLogger _logger;

        public AuthenticationController(IUserAccountService accountService, IFileLogger logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpPost("[action]")]
        public async Task<ApiResponse> Register([FromBody] AccountRegisterPayload payload)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            try
            {
                var process = await _accountService.RegisterUser(payload.Username, payload.DisplayName, payload.Password, payload.Email, payload.PhoneNumber, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while rigistering the user, try again later." };
            }
        }

        [HttpPost("[action]")]
        public async Task<ApiResponse> Login([FromBody] AccountLoginPayload payload)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            try
            {
                var process = await _accountService.UserLogin(payload.Username, payload.Password, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                SessionHelper.SetCurrentUser(HttpContext, process.Data);

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage, Data = process.Data };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while authenticating user, try again later." };
            }
        }
    }
}
