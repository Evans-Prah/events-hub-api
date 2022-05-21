using DBHelper;
using Entities.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTest.Mocks
{
    public class MockPostgresHelper : IPostgresHelper
    {
        private readonly MockDatabase _db;

        public MockPostgresHelper(MockDatabase db)
        {
            _db = db;
        }

        public async Task<EventDetails?> GetEventDetails(string eventUuid)
        {
            return _db.EventDetails.FirstOrDefault(e => e.EventUuid == eventUuid);
        }

        public async Task<List<EventsInfo>> GetEvents()
        {
            return _db.Events;
        }
    }
}
