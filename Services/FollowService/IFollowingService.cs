using Entities;
using Entities.UserFollowing;
using System.Text;

namespace Services.FollowService
{
    public interface IFollowingService
    {
        Task<ServiceResponse> FollowOrUnfollowUser(string observerUsername, string targetUsername, StringBuilder logs);
        Task<List<Followers>> GetUserFollowers(string username);
        Task<List<Followings>> GetUserFollowings(string username);
    }
}