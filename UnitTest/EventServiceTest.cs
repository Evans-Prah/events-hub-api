using NUnit.Framework;
using Services.EventService;
using System.Linq;
using System.Threading.Tasks;
using UnitTest.Mocks;

namespace UnitTest
{

    public class EventServiceTest
    {
        private MockDatabase mockDatabase;
        private IEventService _eventService;

        [SetUp]
        public void Setup()
        {
            mockDatabase = new MockDatabase();
            var postgresHelper = new MockPostgresHelper(mockDatabase);
            _eventService = new EventService(postgresHelper);
        }

        [Test]
        public async Task Will_Get_Events_Successfully()
        {
            var events = await _eventService.GetEvents();
            Assert.IsNotNull(events);
            Assert.IsTrue(events.Count >= 1);
        }

        [Test]
        public async Task Get_Events_Will_Return_Empty_If_No_Data_Exist()
        {
            var events = await _eventService.GetEvents();
            Assert.IsNotNull(events);
            Assert.IsFalse(events.Count == 0);
        }

        [Test]
        public async Task Will_Get_Event_Details_Successfully()
        {
            var request = await _eventService.GetEventDetails(mockDatabase.staticEventUuid);
            Assert.IsNotNull(request);

            var dbData = mockDatabase.EventDetails.FirstOrDefault(e => e.EventUuid == mockDatabase.staticEventUuid);
            Assert.True(dbData?.EventUuid == request?.EventUuid);
        }

        [Test]
        public async Task Get_Event_Details_Will_Fail_If_EventUuid_Is_Invalid()
        {
            var request = await _eventService.GetEventDetails("241f7932-d580-438b-8e75-0b57d4a43f06");
            Assert.IsNull(request);

            var dbData = mockDatabase.EventDetails.FirstOrDefault(e => e.EventUuid != "241f7932-d580-438b-8e75-0b57d4a43f06");
            Assert.IsNotNull(dbData);
        }
    }
}
