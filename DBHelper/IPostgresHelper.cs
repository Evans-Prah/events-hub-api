using Entities.Event;

namespace DBHelper
{
    public interface IPostgresHelper
    {
        Task<string> CreateEvent(string eventUuid, string title, string description, string category, string city, string venue, DateTime date);
        Task<string> DeleteEvent(string eventUuid);
        Task<EventDetails?> GetEventDetails(string eventUuid);
        Task<List<EventsInfo>> GetEvents();
        Task<string> UpdateEvent(string eventUuid, string title, string description, string category, string city, string venue, DateTime date);
    }
}