using Entities.Event;
using System;
using System.Collections.Generic;

namespace UnitTest.Mocks
{
    public class MockDatabase
    {
        public List<EventsInfo> Events;
        public List<EventDetails> EventDetails;

        public string staticEventUuid = "977e33d7-072c-45eb-85b8-30230dd4eb13";

        public MockDatabase()
        {
            Events = new List<EventsInfo>();
            EventDetails = new List<EventDetails>();

            SeedEventsData();
            SeedEventDetails();
        }

        private void SeedEventsData()
        {
            Events.Add(new EventsInfo
            {
                EventUuid = "977e33d7-072c-45eb-85b8-30230dd4eb13",
                Title = "Past Event 1",
                Description = "A drink up event",
                Category = "Drinks",
                City = "Osu",
                Venue = "Pub",
                Date = DateTime.Now.AddMonths(-2),
            });

            Events.Add(new EventsInfo
            {
                EventUuid = "bce1c5d5-6f0a-46a7-bd2b-67203f768fdc",
                Title = "Outdoor Games",
                Description = "An outdoor fun games",
                Category = "Sport",
                City = "Cape Coast",
                Venue = "Cape Coast stadium",
                Date = DateTime.Now.AddMonths(2),
            });
        }

        private void SeedEventDetails()
        {
            EventDetails.Add( new EventDetails
            {
                EventUuid = staticEventUuid,
                Title = "Past Event 1",
                Description = "A drink up event",
                Category = "Drinks",
                City = "Osu",
                Venue = "Pub",
                Date = DateTime.Now.AddMonths(-2),
            });

            Events.Add(new EventsInfo
            {
                EventUuid = "bce1c5d5-6f0a-46a7-bd2b-67203f768fdc",
                Title = "Outdoor Games",
                Description = "An outdoor fun games",
                Category = "Sport",
                City = "Cape Coast",
                Venue = "Cape Coast stadium",
                Date = DateTime.Now.AddMonths(2),
            });
        }
    }


}
