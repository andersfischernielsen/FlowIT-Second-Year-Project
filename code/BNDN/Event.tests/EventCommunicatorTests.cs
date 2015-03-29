using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Event.Controllers;
using Event.Models;
using NUnit.Framework;

namespace Event.tests
{
    [TestFixture]
    public class EventCommunicatorTests
    {
        [Test]
        public void ConstructorRuns()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"));
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void IsExecutedFailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"));
            
            try
            {
                var result = eventCommunicator.IsExecuted().Result;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void IsIncludedFailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"));
            try
            {
                var result = eventCommunicator.IsIncluded().Result;
            }
            catch (Exception ex)
            {
                
                throw ex.InnerException;
            }
        }

    }
}
