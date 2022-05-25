using Entities;
using Entities.Event;
using Entities.Profile;
using Entities.UserAccount;

namespace DBHelper
{
    public interface IPostgresHelper
    {
        Task<string> CreateEvent(string username, string eventUuid, string title, string description, string category, string city, string venue, DateTime date);
        Task<string> DeleteEvent(string username, string eventUuid);
        Task<string> DeleteImage(string username, string publicId);
        Task<EventDetails?> GetEventDetails(string eventUuid);
        Task<List<EventsInfo>> GetEvents();
        Task<UserProfile?> GetUserProfile(string username);
        Task<string> RegisterUser(string accountUuid, string username, string displayName, string password, string email, string phoneNumber);
        Task<string> SetProfilePicture(string username, string publicId);
        Task<string> UpdateEvent(string username, string eventUuid, string title, string description, string category, string city, string venue, DateTime date);
        Task<DbResponse> UpdateEventAttendance(string eventUuid, string username);
        Task<ImageUploadDbResponse> UploadImage(string username, string publicId, string file);
        Task<LoginResponse> UserLogin(string username, string password);
    }
}