using Entities;
using Entities.Payload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.EventService;
using Services.FileLogger;
using System.Text;

namespace API.Controllers
{
    [Route("api/event")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IFileLogger _logger;

        public EventController(IEventService eventService, IFileLogger logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpGet("[action]")]
        public async Task<ApiResponse> GetEvents()
        {
            try
            {
                var events = await _eventService.GetEvents();

                if (events == null || !events.Any()) return new ApiResponse { Success = false, ResponseMessage = "There are no events at the moment" };

                return new ApiResponse { Success = true, ResponseMessage = "Events fetched successfully", Data = events };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while fetching events, try again later." };
            }
        }

        
        [HttpGet("[action]/{eventUuid}")]
        public async Task<ApiResponse> GetEventDetails(string eventUuid)
        {
            try
            {
                var request = await _eventService.GetEventDetails(eventUuid);

                if (request == null) return new ApiResponse { Success = false, ResponseMessage = "Event does not exist, check and try again" };

                return new ApiResponse { Success = true, ResponseMessage = "Event details fetched successfully", Data = request };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while fetching details of event, try again later." };
            }
        }

        [HttpPost("[action]")]
        public async Task<ApiResponse> CreateEvent([FromBody] CreateEventPayload payload)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            if (!ModelState.IsValid) return new ApiResponse { Success = false, ResponseMessage = "Invalid payload sent, check and try again" };

            try
            {
                var process = await _eventService.CreateEvent(payload.Title, payload.Description, payload.Category, payload.City, payload.Venue, payload.Date, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while creating the event, try again later." };
            }
        }
        
        [HttpPost("[action]/{eventUuid}")]
        public async Task<ApiResponse> UpdateEvent([FromBody] UpdateEventPayload payload)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            if (!ModelState.IsValid) return new ApiResponse { Success = false, ResponseMessage = "Invalid payload sent, check and try again" };

            try
            {
                var process = await _eventService.UpdateEvent(payload.EventUuid, payload.Title, payload.Description, payload.Category, payload.City, payload.Venue, payload.Date, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while updating the event, try again later." };
            }
        }
        
        [HttpPost("[action]/{eventUuid}")]
        public async Task<ApiResponse> DeleteEvent(string eventUuid)
        {
            StringBuilder logs = new();
            logs.AppendLine($"Request @ {DateTime.Now}, Path: {Request.Path}");

            try
            {
                var process = await _eventService.DeleteEvent(eventUuid, logs);

                if (!process.Successful) return new ApiResponse { Success = false, ResponseMessage = process.ResponseMessage };

                return new ApiResponse { Success = true, ResponseMessage = process.ResponseMessage };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured while deleting the event, try again later." };
            }
        }
    }
}
