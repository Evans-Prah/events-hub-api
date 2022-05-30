using DBHelper;
using Entities;
using Entities.UserFollowing;
using Newtonsoft.Json;
using System.Text;

namespace Services.FollowService
{
    public class FollowingService : IFollowingService
    {
        private readonly IPostgresHelper _postgresHelper;

        public FollowingService(IPostgresHelper postgresHelper)
        {
            _postgresHelper = postgresHelper;
        }

        public async Task<ServiceResponse> FollowOrUnfollowUser(string observerUsername, string targetUsername, StringBuilder logs)
        {
            logs.AppendLine("-- FollowOrUnfollowUser");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { observerUsername, targetUsername })}");

            if (string.IsNullOrWhiteSpace(observerUsername)) return new ServiceResponse { Successful = false, ResponseMessage = "Your username is required" };
            if (string.IsNullOrWhiteSpace(targetUsername)) return new ServiceResponse { Successful = false, ResponseMessage = "Target username is required" };

            var dbResponse = await _postgresHelper.FollowOrUnfollowUser(observerUsername, targetUsername);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse.ResponseMessage) && dbResponse.ResponseCode == 300) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse.ResponseMessage };
            if (!string.IsNullOrWhiteSpace(dbResponse.ResponseMessage) && dbResponse.ResponseCode == 301) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse.ResponseMessage };

            if (!string.IsNullOrWhiteSpace(dbResponse.ResponseMessage) && dbResponse.ResponseCode == 100) return new ServiceResponse { Successful = true, ResponseMessage = dbResponse.ResponseMessage };

            return new ServiceResponse { Successful = true, ResponseMessage = $"You have followed {targetUsername}" };
        }

        public async Task<List<Following>> GetUserFollowings(string username) => await _postgresHelper.GetUserFollowings(username);
        public async Task<List<Followers>> GetUserFollowers(string username) => await _postgresHelper.GetUserFollowers(username);
    }
}
