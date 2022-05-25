using DBHelper;
using Entities;
using Entities.Profile;

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
                    Photos = dbResponse?.Photos
                }
            };
        }
    }
}
