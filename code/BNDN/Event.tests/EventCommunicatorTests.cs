using System;
using System.Net.Http;
using Event.Communicators;
using NUnit.Framework;

namespace Event.tests
{
    [TestFixture]
    public class EventCommunicatorTests
    {
        [Test]
        public void Constructor_Runs()
        {
            var eventCommunicator = new EventCommunicator();
        }

        [Test]
        [ExpectedException(typeof(HttpRequestException))]
        public void IsExecuted_FailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator();
            
            try
            {
                var result = eventCommunicator.IsExecuted(new Uri("http://test.dk/"), "TargetID", "SenderId").Result;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(HttpRequestException))]
        public void IsIncluded_FailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator();
            try
            {
                var result = eventCommunicator.IsIncluded(new Uri("http://test.dk/"), "TargetID", "SenderId").Result;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
