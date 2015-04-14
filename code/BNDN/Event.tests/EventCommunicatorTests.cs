﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Common;
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
        [ExpectedException(typeof(HttpRequestException))]
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
        [ExpectedException(typeof(HttpRequestException))]
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
        [ExpectedException(typeof(HttpRequestException))]
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
        [ExpectedException(typeof(HttpRequestException))]
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
        [ExpectedException(typeof(HttpRequestException))]
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
        [ExpectedException(typeof(HttpRequestException))]
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
        [ExpectedException(typeof(HttpRequestException))]
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
