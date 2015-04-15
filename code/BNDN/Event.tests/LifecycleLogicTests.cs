using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;
using Event.Logic;
using NUnit.Framework;

namespace Event.tests
{
    [TestFixture]
    class LifecycleLogicTests
    {

        #region Setup

        [TestFixtureSetUp]
        public void Setup()
        {


        }

        #endregion


        #region CreateEvent tests

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateEvent_CalledWithNullEventDto()
        {
            // Arrange
            ILifecycleLogic lifecycleLogic = new LifecycleLogic();
            var uri = new Uri("http://www.dr.dk");

            // Act

            await lifecycleLogic.CreateEvent(null, uri);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task CreateEvent_CalledWithNullUri()
        {
            // Arrange
            ILifecycleLogic lifecycleLogic = new LifecycleLogic();
            var eventDto = new EventDto()
            {
                Conditions = new List<EventAddressDto>(),
                EventId = "Check in",
                Exclusions = new List<EventAddressDto>(),
                Executed = false,
                Included = true,
                Inclusions = new List<EventAddressDto>(),
                Name = "Check in at hospital",
                Pending = false,
                Responses = new List<EventAddressDto>(),
                Roles = new List<string>(),
                WorkflowId = "Cancer surgery"
            };

            // Act
            await lifecycleLogic.CreateEvent(eventDto, null);
        }

        [Test]
        public async Task CreateEvent_WithIdAlreadyInDatabase()
        {
            
        }

        #endregion



    }
}
