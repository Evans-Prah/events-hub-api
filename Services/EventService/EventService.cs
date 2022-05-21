using DBHelper;
using Entities;
using Entities.Event;
using Newtonsoft.Json;
using System.Text;

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

        public async Task<ServiceResponse> CreateEvent(string title, string description, string category, string city, string venue, DateTime date, StringBuilder logs)
        {
            logs.AppendLine("-- CreateEvent");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { title, description, category, city, venue, date })}");

            if (string.IsNullOrWhiteSpace(title)) return new ServiceResponse { Successful = false, ResponseMessage = "Title of event is required" };
            if (string.IsNullOrWhiteSpace(description)) return new ServiceResponse { Successful = false, ResponseMessage = "Provide description for the event" };
            if (string.IsNullOrWhiteSpace(category)) return new ServiceResponse { Successful = false, ResponseMessage = "Event category is reqired" };
            if (string.IsNullOrWhiteSpace(city)) return new ServiceResponse { Successful = false, ResponseMessage = "Event city is required" };
            if (string.IsNullOrWhiteSpace(venue)) return new ServiceResponse { Successful = false, ResponseMessage = "Venue for the event is required" };
            if (date < DateTime.Now) return new ServiceResponse { Successful = false, ResponseMessage = "Date and time for event cannot be less than current date and time" };

            var eventUuid = Guid.NewGuid().ToString();

            var dbResponse = await _postgresHelper.CreateEvent(eventUuid, title, description, category, city, venue, date);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            return new ServiceResponse { Successful = true, ResponseMessage = "Event has been created successfully" };
        }
        
        public async Task<ServiceResponse> UpdateEvent(string eventUuid, string title, string description, string category, string city, string venue, DateTime date, StringBuilder logs)
        {
            logs.AppendLine("-- UpdateEvent");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { eventUuid, title, description, category, city, venue, date })}");

            if (string.IsNullOrWhiteSpace(eventUuid)) return new ServiceResponse { Successful = false, ResponseMessage = "Request Identifer (EventUuid) for event is required" };
            if (string.IsNullOrWhiteSpace(title)) return new ServiceResponse { Successful = false, ResponseMessage = "Title of event is required" };
            if (string.IsNullOrWhiteSpace(description)) return new ServiceResponse { Successful = false, ResponseMessage = "Provide description for the event" };
            if (string.IsNullOrWhiteSpace(category)) return new ServiceResponse { Successful = false, ResponseMessage = "Event category is reqired" };
            if (string.IsNullOrWhiteSpace(city)) return new ServiceResponse { Successful = false, ResponseMessage = "Event city is required" };
            if (string.IsNullOrWhiteSpace(venue)) return new ServiceResponse { Successful = false, ResponseMessage = "Venue for the event is required" };
            if (date < DateTime.Now) return new ServiceResponse { Successful = false, ResponseMessage = "Date and time for event cannot be less than current date and time" };

            var dbResponse = await _postgresHelper.UpdateEvent(eventUuid, title, description, category, city, venue, date);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if(!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            return new ServiceResponse { Successful = true, ResponseMessage = "Event has been updated successfully" };
        }
        
        public async Task<ServiceResponse> DeleteEvent(string eventUuid, StringBuilder logs)
        {
            logs.AppendLine("-- DeleteEvent");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { eventUuid })}");

            if (string.IsNullOrWhiteSpace(eventUuid)) return new ServiceResponse { Successful = false, ResponseMessage = "Request Identifer (EventUuid) for event is required" };
            
            var dbResponse = await _postgresHelper.DeleteEvent(eventUuid);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if(!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            return new ServiceResponse { Successful = true, ResponseMessage = "Event has been deleted successfully" };
        }
    }
}