using System;
using Common.Exceptions;
using Event.Communicators;
using NUnit.Framework;

namespace Event.Tests
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
        [ExpectedException(typeof(NotFoundException))]
        public void IsExecuted_FailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator();
            
            try
            {
                var result = eventCommunicator.IsExecuted(new Uri("http://test.dk/"), "targetWorkflowId", "TargetID", "SenderId").Result;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(NotFoundException))]
        public void IsIncluded_FailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator();
            try
            {
                var result = eventCommunicator.IsIncluded(new Uri("http://test.dk/"), "targetWorkflowId", "TargetID", "SenderId").Result;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
