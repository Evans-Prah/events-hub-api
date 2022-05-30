using Entities;
using Entities.Event;
using Entities.Profile;
using Entities.UserAccount;
using Entities.UserFollowing;

namespace DBHelper
{
    public interface IPostgresHelper
    {
        Task<string> AddEventComment(string username, string eventUuid, string comment);
        Task<string> CreateEvent(string username, string eventUuid, string title, string description, string category, string city, string venue, DateTime date);
        Task<string> DeleteEvent(string username, string eventUuid);
        Task<string> DeleteImage(string username, string publicId);
        Task<FollowingDbResponse> FollowOrUnfollowUser(string observerUsername, string targetUsername);
        Task<List<EventComment>> GetEventComments(string eventUuid);
        Task<EventDetails?> GetEventDetails(string eventUuid);
        Task<List<EventsInfo>> GetEvents();
        Task<List<Followers>> GetUserFollowers(string username);
        Task<List<Following>> GetUserFollowings(string username);
        Task<UserProfile?> GetUserProfile(string username);
        Task<string> RegisterUser(string accountUuid, string username, string displayName, string password, string email, string phoneNumber);
        Task<string> SetProfilePicture(string username, string publicId);
        Task<string> UpdateEvent(string username, string eventUuid, string title, string description, string category, string city, string venue, DateTime date);
        Task<DbResponse> UpdateEventAttendance(string eventUuid, string username);
        Task<string> UpdateUserProfile(string username, string displayName, string bio);
        Task<ImageUploadDbResponse> UploadImage(string username, string publicId, string file);
        Task<LoginResponse> UserLogin(string username, string password);
    }
}