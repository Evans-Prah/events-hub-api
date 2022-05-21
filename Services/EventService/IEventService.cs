using Entities;
using Entities.Event;
using System.Text;

namespace Services.EventService
{
    public interface IEventService
    {
        Task<ServiceResponse> CreateEvent(string title, string description, string category, string city, string venue, DateTime date, StringBuilder logs);
        Task<ServiceResponse> DeleteEvent(string eventUuid, StringBuilder logs);
        Task<EventDetails?> GetEventDetails(string eventUuid);
        Task<List<EventsInfo>> GetEvents();
        Task<ServiceResponse> UpdateEvent(string eventUuid, string title, string description, string category, string city, string venue, DateTime date, StringBuilder logs);
    }
}