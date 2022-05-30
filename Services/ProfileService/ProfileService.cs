using DBHelper;
using Entities;
using Entities.Profile;
using Newtonsoft.Json;
using System.Text;

namespace Services.ProfileService
{
    public class ProfileService : IProfileService
    {
        private readonly IPostgresHelper _postgresHelper;

        public ProfileService(IPostgresHelper postgresHelper)
        {
            _postgresHelper = postgresHelper;
        }

        public async Task<ServiceResponse> GetUserProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username of the user's profile is required" };

            var dbResponse = await _postgresHelper.GetUserProfile(username);

            if (!string.IsNullOrWhiteSpace(dbResponse?.ResponseMessage)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse?.ResponseMessage };

            return new ServiceResponse
            {
                Successful = true,
                ResponseMessage = $"{dbResponse?.Username} profile fetched successfully",
                Data = new UserProfile
                {
                    ResponseMessage = $"{dbResponse?.Username} profile fetched successfully",
                    Username = dbResponse?.Username,
                    DisplayName = dbResponse?.DisplayName,
                    Bio = dbResponse?.Bio,
                    Image = dbResponse?.Image,
                    FollowersCount = dbResponse.FollowersCount,
                    FollowingCount = dbResponse.FollowingCount,
                    Photos = dbResponse?.Photos
                }
            };
        }

        public async Task<ServiceResponse> UpdateUserProfile(string username, string displayName, string bio, StringBuilder logs)
        {
            logs.AppendLine("-- UpdateUserProfile");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, displayName, bio })}");

            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(displayName)) return new ServiceResponse { Successful = false, ResponseMessage = "Your display name is required" };
            if (string.IsNullOrWhiteSpace(bio)) return new ServiceResponse { Successful = false, ResponseMessage = "Your bio is required" };

            var dbResponse = await _postgresHelper.UpdateUserProfile(username, displayName, bio);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            return new ServiceResponse { Successful = true, ResponseMessage = "User profile updated successfully" };
        }
    }
}
