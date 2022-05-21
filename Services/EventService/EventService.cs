using DBHelper;
using Entities.Event;

namespace Services.EventService
{
    public class EventService : IEventService
    {
        private readonly IPostgresHelper _postgresHelper;

        public EventService(IPostgresHelper postgresHelper)
        {
            _postgresHelper = postgresHelper;
        }

        public async Task<List<EventsInfo>> GetEvents() => await _postgresHelper.GetEvents();

        public async Task<EventDetails?> GetEventDetails(string eventUuid) => await _postgresHelper.GetEventDetails(eventUuid);
    }
}
