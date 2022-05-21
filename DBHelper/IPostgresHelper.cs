using Entities.Event;

namespace DBHelper
{
    public interface IPostgresHelper
    {
        Task<EventDetails?> GetEventDetails(string eventUuid);
        Task<List<EventsInfo>> GetEvents();
    }
}