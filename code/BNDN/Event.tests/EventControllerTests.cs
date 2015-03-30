using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Event.Controllers;
using NUnit.Framework;

namespace Event.tests
{
    [TestFixture]
    public class EventControllerTests
    {
        private EventController _eventController;

        [SetUp]
        public void Setup()
        {
            EventController eventController = new EventController();
            _eventController = eventController;

            EventDto testEvent1 = new EventDto(){EventId = "1", WorkflowId = "1", Name = "TestEvent1"};
        }

        [Test]
        public void GetEventTest()
        {

        }

    }
}
