using Entities.Event;

namespace Services.EventService
{
    public interface IEventService
    {
        Task<EventDetails?> GetEventDetails(string eventUuid);
        Task<List<EventsInfo>> GetEvents();
    }
}