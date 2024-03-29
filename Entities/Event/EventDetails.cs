﻿namespace Entities.Event
{
    public class EventDetails
    {
        public string? EventUuid { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? City { get; set; }
        public string? Venue { get; set; }
        public DateTime Date { get; set; }
        public string? HostUsername { get; set; }
        public bool IsCancelled { get; set; }
        public List<AttendeesProfile>? Attendees { get; set; }
        public long Likes { get; set; }
        public long Comments { get; set; }
    }
}
