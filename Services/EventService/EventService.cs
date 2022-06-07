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

        public async Task<ServiceResponse> CreateEvent(string username, string title, string description, string category, string city, string venue, DateTime date, StringBuilder logs)
        {
            logs.AppendLine("-- CreateEvent");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, title, description, category, city, venue, date })}");

            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(title)) return new ServiceResponse { Successful = false, ResponseMessage = "Title of event is required" };
            if (string.IsNullOrWhiteSpace(description)) return new ServiceResponse { Successful = false, ResponseMessage = "Provide description for the event" };
            if (string.IsNullOrWhiteSpace(category)) return new ServiceResponse { Successful = false, ResponseMessage = "Event category is reqired" };
            if (string.IsNullOrWhiteSpace(city)) return new ServiceResponse { Successful = false, ResponseMessage = "Event city is required" };
            if (string.IsNullOrWhiteSpace(venue)) return new ServiceResponse { Successful = false, ResponseMessage = "Venue for the event is required" };
            if (date < DateTime.Now) return new ServiceResponse { Successful = false, ResponseMessage = "Date and time for event cannot be less than current date and time" };

            var eventUuid = Guid.NewGuid().ToString();

            var dbResponse = await _postgresHelper.CreateEvent(username, eventUuid, title, description, category, city, venue, date);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            return new ServiceResponse { Successful = true, ResponseMessage = "Event has been created successfully" };
        }
        
        public async Task<ServiceResponse> UpdateEvent(string username, string eventUuid, string title, string description, string category, string city, string venue, DateTime date, StringBuilder logs)
        {
            logs.AppendLine("-- UpdateEvent");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, eventUuid, title, description, category, city, venue, date })}");

            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(eventUuid)) return new ServiceResponse { Successful = false, ResponseMessage = "Request Identifer (EventUuid) for event is required" };
            if (string.IsNullOrWhiteSpace(title)) return new ServiceResponse { Successful = false, ResponseMessage = "Title of event is required" };
            if (string.IsNullOrWhiteSpace(description)) return new ServiceResponse { Successful = false, ResponseMessage = "Provide description for the event" };
            if (string.IsNullOrWhiteSpace(category)) return new ServiceResponse { Successful = false, ResponseMessage = "Event category is reqired" };
            if (string.IsNullOrWhiteSpace(city)) return new ServiceResponse { Successful = false, ResponseMessage = "Event city is required" };
            if (string.IsNullOrWhiteSpace(venue)) return new ServiceResponse { Successful = false, ResponseMessage = "Venue for the event is required" };
            if (date < DateTime.Now) return new ServiceResponse { Successful = false, ResponseMessage = "Date and time for event cannot be less than current date and time" };

            var dbResponse = await _postgresHelper.UpdateEvent(username, eventUuid, title, description, category, city, venue, date);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if(!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            return new ServiceResponse { Successful = true, ResponseMessage = "Event has been updated successfully" };
        }
        
        public async Task<ServiceResponse> DeleteEvent(string username, string eventUuid, StringBuilder logs)
        {
            logs.AppendLine("-- DeleteEvent");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, eventUuid })}");

            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(eventUuid)) return new ServiceResponse { Successful = false, ResponseMessage = "Request Identifer (EventUuid) for event is required" };
            
            var dbResponse = await _postgresHelper.DeleteEvent(username, eventUuid);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if(!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            return new ServiceResponse { Successful = true, ResponseMessage = "Event has been deleted successfully" };
        }
        
        public async Task<ServiceResponse> UpdateEventAttendance(string eventUuid, string username, StringBuilder logs)
        {
            logs.AppendLine("-- UpdateEventAttendance");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { eventUuid, username })}");

            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(eventUuid)) return new ServiceResponse { Successful = false, ResponseMessage = "Request Identifer (EventUuid) for event is required" };
            
            var dbResponse = await _postgresHelper.UpdateEventAttendance(eventUuid, username);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if(!string.IsNullOrWhiteSpace(dbResponse.Message) && dbResponse.ResponseCode == 400) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse.Message, Data = dbResponse.ResponseCode };
            if(!string.IsNullOrWhiteSpace(dbResponse.Message) && dbResponse.ResponseCode == 402) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse.Message, Data = dbResponse.ResponseCode };
            
            if(!string.IsNullOrWhiteSpace(dbResponse.Message) && dbResponse.ResponseCode == 100) return new ServiceResponse { Successful = true, ResponseMessage = dbResponse.Message, Data = dbResponse.ResponseCode };
            if(!string.IsNullOrWhiteSpace(dbResponse.Message) && dbResponse.ResponseCode == 102) return new ServiceResponse { Successful = true, ResponseMessage = dbResponse.Message, Data = dbResponse.ResponseCode };

            return new ServiceResponse { Successful = true, ResponseMessage = "You have been added to the event attendees", Data = dbResponse.ResponseCode };
        }

        public async Task<ServiceResponse> AddEventComment(string username, string eventUuid, string comment, StringBuilder logs)
        {
            logs.AppendLine("-- AddEventComment");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, eventUuid, comment })}");

            if (string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(eventUuid)) return new ServiceResponse { Successful = false, ResponseMessage = "Request Identifer (EventUuid) for event is required" };
            if (string.IsNullOrWhiteSpace(comment)) return new ServiceResponse { Successful = false, ResponseMessage = "Comment is required" };

            var dbResponse = await _postgresHelper.AddEventComment(username, eventUuid, comment);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse };

            return new ServiceResponse { Successful = true, ResponseMessage = "Comment added successfully" };
        }

        public async Task<List<EventComment>> GetEventComments(string eventUuid) => await _postgresHelper.GetEventComments(eventUuid);

        public async Task<List<EventLikes>> GetEventLikes(string eventUuid) => await _postgresHelper.GetEventLikes(eventUuid);

        public async Task<ServiceResponse> LikeOrUnlikeEvent(string eventUuid, string username, StringBuilder logs)
        {
            logs.AppendLine("-- LikeOrUnlikeEvent");
            logs.AppendLine($"Payload: {JsonConvert.SerializeObject(new { username, eventUuid })}");

            if(string.IsNullOrWhiteSpace(username)) return new ServiceResponse { Successful = false, ResponseMessage = "Username is required" };
            if (string.IsNullOrWhiteSpace(eventUuid)) return new ServiceResponse { Successful = false, ResponseMessage = "Request Identifer (EventUuid) for event is required" };

            var dbResponse = await _postgresHelper.LikeOrUnlikeEvent(eventUuid, username);
            logs.AppendLine($"DB Response: {JsonConvert.SerializeObject(dbResponse)}");

            if (!string.IsNullOrWhiteSpace(dbResponse.Message)) return new ServiceResponse { Successful = false, ResponseMessage = dbResponse.Message, Data = dbResponse.ResponseCode };
            if (!string.IsNullOrWhiteSpace(dbResponse.Message) && dbResponse.ResponseCode == 100) return new ServiceResponse { Successful = true, ResponseMessage = dbResponse.Message, Data = dbResponse.ResponseCode };

            return new ServiceResponse { Successful = true, ResponseMessage = "You liked the event" };
        }
    }
}