﻿using DBHelper;
using Entities;
using Entities.UserAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Services.EmailService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.UserAccount
{
    public class UserAccountService : IUserAccountService
    {
        private readonly IPostgresHelper _postgresHelper;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSenderService _emailSenderService;
        private readonly string _encryption_key;
        private readonly string _jwt_encryption_key;
        private readonly double _session_timeout;

        public UserAccountService(IPostgresHelper postgresHelper, IConfiguration config, IHttpContextAccessor httpContextAccessor, IEmailSenderService emailSenderService)
        {
            _postgresHelper = postgresHelper;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _emailSenderService = emailSenderService;
            _encryption_key = _config["ENCRYPTION_KEY"];
            _jwt_encryption_key = _config["JWT_ENCRYPTION_KEY"];
            _session_timeout = Convert.ToDouble(_config["SESSION_TIMEOUT"]);
        }

        public async Task<ServiceResponse> RegisterUser(string username, string displayName, string password, string email, string phoneNumber, StringBuilder logs)
        {
            logs.AppendLine("-- CreateAccount");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, email, phoneNumber })}");

            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(displayName)) return new ServiceResponse { Successful = false, ResponseMessage = "Dsiplay name is required" };
            if (string.IsNullOrWhiteSpace(password)) return new ServiceResponse { Successful = false, ResponseMessage = "Password is required" };
            if (string.IsNullOrWhiteSpace(email)) return new ServiceResponse { Successful = false, ResponseMessage = "Email is required" };
            if (string.IsNullOrWhiteSpace(phoneNumber)) return new ServiceResponse { Successful = false, ResponseMessage = "Phone number is required" };

            if (password.Length <= 5) return new ServiceResponse { Successful = false, ResponseMessage = "Password should be at least 6 characters long" };

            if (!StringCipher.IsValidEmail(email)) return new ServiceResponse { Successful = false, ResponseMessage = "Email format is incorrect, enter a valid email format" };

            var accountUuid = Guid.NewGuid().ToString();
            password = StringCipher.Encrypt(password, _encryption_key);

            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();

            var emailConfirmationToken = Convert.ToBase64String(time.Concat(key).ToArray());
            emailConfirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));

            var dbResponse = await _postgresHelper.RegisterUser(accountUuid, username, displayName, password, email, phoneNumber, emailConfirmationToken);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            var origin = _httpContextAccessor.HttpContext.Request.Headers["origin"];

            var verifyEmailUrl = $"{origin}/api/auth/verifyEmail?token={emailConfirmationToken}&email={email}";

            string filePath = Directory.GetCurrentDirectory() + "\\wwwroot\\Templates\\EmailTemplates\\ConfirmAccountRegistration.html";
            StreamReader str = new StreamReader(filePath);
            string mailText = str.ReadToEnd();
            str.Close();

            mailText = mailText
                              .Replace("[TITLE]", "Confirm Account Registration")
                              .Replace("[DATETIME]", string.Format("{0:dddd, d MMMM yyyy}", DateTime.UtcNow))
                              .Replace("[USERNAME]", username)
                              .Replace("[CONFIRM_EMAIL_URL]", verifyEmailUrl)
                              .Replace("[EMAIL]", email)
                              .Replace("[USERNAME]", username);

            var message = $"Please click the link to verify your email address: {verifyEmailUrl}";

            await _emailSenderService.SendEmail(email, "Confirm Account Registration", mailText);

            return new ServiceResponse { Successful = true, ResponseMessage = "User account registered successfully, please verify email address", Data = message };
        }


        public async Task<ServiceResponse> VerifyEmail(string email, string emailConfirmationToken, StringBuilder logs)
        {
            logs.AppendLine("-- VerifyEmail");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { email, emailConfirmationToken })}");

            var dbResponse = await _postgresHelper.VerifyEmail(email, emailConfirmationToken);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            return new ServiceResponse { Successful = true, ResponseMessage = "Email confirmation successful - you can login" };
        }
        
        public async Task<ServiceResponse> ResendEmailConfirmationLink(string email,  StringBuilder logs)
        {
            logs.AppendLine("-- ResendEmailConfirmationLink");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { email})}");

            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();

            var emailConfirmationToken = Convert.ToBase64String(time.Concat(key).ToArray());
            emailConfirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmationToken));

            var dbResponse = await _postgresHelper.ResendEmailConfirmationLink(email, emailConfirmationToken);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            var origin = _httpContextAccessor.HttpContext.Request.Headers["origin"];

            var verifyEmailUrl = $"{origin}/api/auth/verifyEmail?token={emailConfirmationToken}&email={email}";

            string filePath = Directory.GetCurrentDirectory() + "\\wwwroot\\Templates\\EmailTemplates\\ConfirmAccountRegistration.html";
            StreamReader str = new StreamReader(filePath);
            string mailText = str.ReadToEnd();
            str.Close();

            mailText = mailText
                              .Replace("[TITLE]", "Confirm Account Registration")
                              .Replace("[DATETIME]", string.Format("{0:dddd, d MMMM yyyy}", DateTime.UtcNow))
                              .Replace("[USERNAME]", email)
                              .Replace("[CONFIRM_EMAIL_URL]", verifyEmailUrl)
                              .Replace("[EMAIL]", email)
                              .Replace("[USERNAME]", email);

            var message = $"Please click the link to verify your email address: {verifyEmailUrl}";

            await _emailSenderService.SendEmail(email, "Confirm Account Registration", mailText);

            return new ServiceResponse { Successful = true, ResponseMessage = "Email verification link resent", Data = message };
        }


        public async Task<ServiceResponse> UserLogin(string username, string password, StringBuilder logs)
        {
            logs.AppendLine("-- UserLogin");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username })}");

            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(password)) return new ServiceResponse { Successful = false, ResponseMessage = "Password is required" };

            password = StringCipher.Encrypt(password, _encryption_key);

            var dbResponse = await _postgresHelper.UserLogin(username, password);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse.ResponseMessage)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse.ResponseMessage };

            var tokenInfo = CreateJWTInfo(username, AuthRoles.USER);

            return new ServiceResponse
            {
                Successful = true,
                ResponseMessage = "Login successful",
                Data = new LoginResponse
                {
                    Username = dbResponse.Username,
                    DisplayName = dbResponse.DisplayName,
                    ProfilePicture = dbResponse.ProfilePicture,
                    LastLogin = dbResponse.LastLogin,
                    Token = tokenInfo
                }
            };
        } 
        
        public async Task<ServiceResponse> ForgotPassword(string email, StringBuilder logs)
        {
            logs.AppendLine("-- ForgotPassword");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { email })}");

            if (string.IsNullOrWhiteSpace(email)) return new ServiceResponse { Successful = false, ResponseMessage = "Email is required" };

            string passwordResetCode = GeneratePasswordResetCode();

            var dbResponse = await _postgresHelper.ForgotPassword(email, passwordResetCode);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse.Message)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse.Message };

            string filePath = Directory.GetCurrentDirectory() + "\\wwwroot\\Templates\\EmailTemplates\\ForgotPassword.html";
            StreamReader str = new StreamReader(filePath);
            string mailText = str.ReadToEnd();
            str.Close();

            mailText = mailText
                              .Replace("[TITLE]", "Reset Password")
                              .Replace("[DATETIME]", string.Format("{0:dddd, d MMMM yyyy}", DateTime.UtcNow))
                              .Replace("[USERNAME]", dbResponse.Username)
                              .Replace("[OTP]", passwordResetCode)
                              .Replace("[OTP_EXPIRY_TIME]", string.Format("{0:dddd, d MMMM yyyy HH:mm:ss}", Convert.ToDateTime( dbResponse.ResetCodeExpiry)));

            await _emailSenderService.SendEmail(email, "Reset Password", mailText);

            return new ServiceResponse
            {
                Successful = true,
                ResponseMessage = "Password reset code sent successfully",
                Data = new ForgotPasswordResponse
                {
                    Username = dbResponse.Username,
                    Email = dbResponse.Email,
                    ResetCode = passwordResetCode,
                    ResetCodeExpiry = dbResponse.ResetCodeExpiry,
                }
            };
        }

        public async Task<ServiceResponse> VerifyPasswordReset(string email, string passwordResetCode, StringBuilder logs)
        {
            logs.AppendLine("-- VerifyPasswordReset");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { email, passwordResetCode })}");

            if (string.IsNullOrWhiteSpace(email)) return new ServiceResponse { Successful = false, ResponseMessage = "Email is required" };
            if (string.IsNullOrWhiteSpace(passwordResetCode)) return new ServiceResponse { Successful = false, ResponseMessage = "Password reset code is required" };

            var dbResponse = await _postgresHelper.VerifyPasswordReset(email, passwordResetCode);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            return new ServiceResponse { Successful = true, ResponseMessage = "Password reset verification successful - you can proceed to change your password" };
        } 
        
        public async Task<ServiceResponse> ResetPassword(string email, string newPassword, StringBuilder logs)
        {
            logs.AppendLine("-- ResetPassword");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { email })}");

            if (string.IsNullOrWhiteSpace(email)) return new ServiceResponse { Successful = false, ResponseMessage = "Email is required" };
            if (string.IsNullOrWhiteSpace(newPassword)) return new ServiceResponse { Successful = false, ResponseMessage = "New password is required" };

            if (newPassword.Length <= 5) return new ServiceResponse { Successful = false, ResponseMessage = "Password should be at least 6 characters long" };
             
            newPassword = StringCipher.Encrypt(newPassword, _encryption_key);

            var dbResponse = await _postgresHelper.ResetPassword(email, newPassword);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            return new ServiceResponse { Successful = true, ResponseMessage = "Password reset successful - you can proceed to login with your new password" };
        }

        private JwtInfo CreateJWTInfo(string username, string roles)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_session_timeout);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(_jwt_encryption_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Expiration, expiresAt.ToString()),
                    new Claim(ClaimTypes.Role, roles)
                }),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            jwtToken = StringCipher.Encrypt(jwtToken, _jwt_encryption_key);
            return new JwtInfo 
            { 
                Token = jwtToken, 
                ExpiresAt = expiresAt
            };
        }

        private static string GeneratePasswordResetCode()
        {
            Random random = new Random();
            var code = random.Next(100000, 999999).ToString();
            return code;
        }

    }
}
