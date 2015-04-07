﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Controllers;
using Event.Models;
using Event.Storage;
using NUnit.Framework;

namespace Event.tests
{
    [TestFixture]
    public class EventCommunicatorTests
    {
        [Test]
        public void Constructor_Runs()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"), "TargetID", "SenderId");
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void IsExecuted_FailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"), "TargetID", "SenderId");
            
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
        public void IsIncluded_FailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"), "TargetID", "SenderId");
            try
            {
                var result = eventCommunicator.IsIncluded().Result;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void GetEvent_FailsOnWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"), "TargetID", "SenderId");
            try
            {
                var result = eventCommunicator.GetEvent().Result;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void PostEventRules_FailsWithWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"), "TargetID", "SenderId");
            try
            {
                eventCommunicator.PostEventRules(new EventRuleDto()).Wait();
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void UpdateEventRules_FailsWithWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"), "TargetID", "SenderId");
            try
            {
                eventCommunicator.UpdateEventRules(new EventRuleDto()).Wait();
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void DeleteEventRules_FailsWithWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"), "TargetID", "SenderId");
            try
            {
                eventCommunicator.DeleteEventRules().Wait();
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void SendNotify_FailsWithWrongUri()
        {
            var eventCommunicator = new EventCommunicator(new Uri("http://test.dk/"), "TargetID", "SenderId");
            try
            {
                eventCommunicator.SendNotify(new List<NotifyDto>()).Wait();
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

    }
}