﻿using Entities;
using Entities.Event;
using System.Text;

namespace Services.EventService
{
    public interface IEventService
    {
        Task<ServiceResponse> AddEventComment(string username, string eventUuid, string comment, StringBuilder logs);
        Task<ServiceResponse> CreateEvent(string username, string title, string description, string category, string city, string venue, DateTime date, StringBuilder logs);
        Task<ServiceResponse> DeleteEvent(string username, string eventUuid, StringBuilder logs);
        Task<List<EventComment>> GetEventComments(string eventUuid);
        Task<EventDetails?> GetEventDetails(string eventUuid);
        Task<List<EventsInfo>> GetEvents();
        Task<ServiceResponse> UpdateEvent(string username, string eventUuid, string title, string description, string category, string city, string venue, DateTime date, StringBuilder logs);
        Task<ServiceResponse> UpdateEventAttendance(string eventUuid, string username, StringBuilder logs);
    }
}