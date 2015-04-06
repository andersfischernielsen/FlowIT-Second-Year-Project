using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Web.Http.Controllers;
using Common;
using Event.Interfaces;
using Event.Models;
using Event.Storage;
using NUnit.Framework;

namespace Event.tests
{
    [TestFixture]
    class EventLogicTests
    {
        [Test]
        public void SetupLogicIsNotNullTest()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());

            //Act

            //Assert
            Assert.IsNotNull(eventLogic);
        }


        #region Rulehandling
        [Test]
        public void IsExecutable_ShouldReturnTrueWhenNoConditionsExist_Test()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            eventLogic.Included = true;

            //Act
            var result = eventLogic.IsExecutable().Result;

            //Assert
            Assert.AreEqual(true, result);
        }
        [Test]
        public void IsExecutable_ShouldReturnFalseWhenNoConditionsExist_Test()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            eventLogic.Included = false;

            //Act
            var result = eventLogic.IsExecutable().Result;

            //Assert
            Assert.AreEqual(false, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateRulesFailDueToRulesBeingNullTest()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            // TODO: Redesign test to reflect changes in IEventStorage 
            // eventLogic.Storage.Events.Add("Test", new Uri("http://test/"));

            //Act
            try
            {
                eventLogic.UpdateRules("Test", null).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("Value cannot be null.\r\nParameter name: rules", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

        //This fails due to incorrect code.
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void UpdateRulesFailDueToIdNotExistingTest()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());

            //Act
            try
            {
                eventLogic.UpdateRules("Test", new EventRuleDto()).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("Nonexistent id\r\nParameter name: Test", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

        [Test]
        public void UpdateRulesRunsTest()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            // TODO: Redesign test to reflect changes in IEventStorage
            //eventLogic.Storage.EventUris.Add("Test", new Uri("http://test/"));

            Assert.DoesNotThrow(() => eventLogic.UpdateRules("Test", new EventRuleDto()).Wait());
        }

        [Test]
        public void UpdateRuleAddsValueToCollectionTest()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            eventLogic.Conditions = new HashSet<RelationToOtherEventModel>();
            var uri = new RelationToOtherEventModel {Uri = new Uri("http://test/")};
            // TODO: Redesign test to reflect changes in IEventStorage
            //eventLogic.Storage.EventUris.Add("Test", uri);

            //Act
            eventLogic.UpdateRules("Test", new EventRuleDto() {Condition = true}).Wait();
            var result = eventLogic.Conditions.Contains(uri);

            //Assert
            Assert.AreEqual(true, result);
        }

        [Test]
        public void UpdateRuleRemovesValueFromCollectionTest()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            eventLogic.Conditions = new HashSet<RelationToOtherEventModel>();
            var uri = new RelationToOtherEventModel { Uri = new Uri("http://test/") };
            // TODO: Redesign test to reflect changes in IEventStorage
            //eventLogic.Storage.EventUris.Add("Test", uri);
            eventLogic.Conditions.Add(uri);

            //Act
            eventLogic.UpdateRules("Test", new EventRuleDto(){Condition = false}).Wait();
            var result = eventLogic.Conditions.Contains(uri);

            //Assert
            Assert.AreEqual(false, result);
        }
        #endregion

        #region DTO Creation

        [Test]
        public void GetEventStateDtoWhenIncludedIsFalseTest()
        {
            //Arrange
            var eventLogic = new EventLogic();
            eventLogic.Included = false;
            eventLogic.Executed = true;
            eventLogic.Pending = true;

            //Act
            var result = eventLogic.EventStateDto.Result;

            //Assert
            Assert.AreEqual(false,result.Included);
            Assert.AreEqual(true, result.Executed);
            Assert.AreEqual(true, result.Pending);
            Assert.AreEqual(false, result.Executable);

            // We don't know if ID and Name should be null
            Assert.AreEqual(null, result.Name);
            Assert.AreEqual(null, result.Id);
        }
        [Test]
        public void GetEventStateDtoWhenIncludedIsTrueTest()
        {
            //Arrange
            var eventLogic = new EventLogic();
            eventLogic.Included = true;
            eventLogic.Executed = false;
            eventLogic.Pending = false;
          
            //Act
            var result = eventLogic.EventStateDto.Result;

            //Assert
            Assert.AreEqual(true, result.Included);
            Assert.AreEqual(false, result.Executed);
            Assert.AreEqual(false, result.Pending);
            Assert.AreEqual(true, result.Executable);

            // We don't know if ID and Name should be null
            Assert.AreEqual(null, result.Name);
            Assert.AreEqual(null, result.Id);
        }
        #endregion

        [Test]
        public void GetEventDtoPropertyTest()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            eventLogic.Name = "TestName";
            eventLogic.EventId = "TestId";
            eventLogic.WorkflowId = "TestWId";
            eventLogic.Included = true;
            eventLogic.Executed = false;
            eventLogic.Pending = false;

            //Act
            var result = eventLogic.EventDto.Result;

            //Assert
            Assert.AreEqual(true, result.Included);
            Assert.AreEqual(false, result.Executed);
            Assert.AreEqual(false, result.Pending);
            Assert.AreEqual("TestWId", result.WorkflowId);
            Assert.AreEqual("TestName", result.Name);
            Assert.AreEqual("TestId", result.EventId);
        }


        #region URI registration

        [Test]
        public void ResetStateTest()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            eventLogic.Name = "TestName";
            eventLogic.EventId = "TestId";
            eventLogic.WorkflowId = "TestWId";
            eventLogic.OwnUri = new Uri("http://test/");

            //Act
            eventLogic.ResetState().Wait();

            //Assert
            Assert.IsNull(eventLogic.Name);
            Assert.IsNull(eventLogic.EventId);
            Assert.IsNull(eventLogic.WorkflowId);
            Assert.IsNull(eventLogic.OwnUri);
        }

        [Test]
        //This test only tests that values are set, and not a connection to the server is established.
        public void InitializeEventRuns()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            var eventDto = new EventDto()
            {
                EventId = "TestId",
                WorkflowId = "TestWId",
                Name = "TestName",
                Included = true,
                Pending = true,
                Executed = true,
                Inclusions = new HashSet<EventAddressDto>(),
                Exclusions = new HashSet<EventAddressDto>(),
                Conditions = new HashSet<EventAddressDto>(),
                Responses = new HashSet<EventAddressDto>(),
            };

            //Act
            try
            {
                eventLogic.InitializeEvent(eventDto, new Uri("http://test/")).Wait();
            }
            catch (Exception)
            {
                // ignored
            }


            //Assert
            Assert.AreEqual(true, eventLogic.Included);
            Assert.AreEqual(true, eventLogic.Executed);
            Assert.AreEqual(true, eventLogic.Pending);
            Assert.AreEqual("TestWId", eventLogic.WorkflowId);
            Assert.AreEqual("TestName", eventLogic.Name);
            Assert.AreEqual("TestId", eventLogic.EventId);
            Assert.AreEqual(new HashSet<Uri>(), eventLogic.Inclusions);
            Assert.AreEqual(new HashSet<Uri>(), eventLogic.Exclusions);
            Assert.AreEqual(new HashSet<Uri>(), eventLogic.Conditions);
            Assert.AreEqual(new HashSet<Uri>(), eventLogic.Responses);
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void InitializeEventEventDtoIsNull()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());

            //Act
            try
            {
                eventLogic.InitializeEvent(null, new Uri("http://test/")).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("Provided EventDto was null", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void InitializeEventEventIdIsNotNull()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            eventLogic.EventId = "Test";

            //Act
            try
            {
                eventLogic.InitializeEvent(new EventDto(), new Uri("http://test/")).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("EventId was not null", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

        [Test]
        //This test only tests that values are set, and not a connection to the server is established.
        public void UpdateEventRuns()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            var eventDto = new EventDto()
            {
                EventId = "TestId",
                WorkflowId = "TestWId",
                Name = "TestName",
                Included = true,
                Pending = true,
                Executed = true,
                Inclusions = new HashSet<EventAddressDto>(),
                Exclusions = new HashSet<EventAddressDto>(),
                Conditions = new HashSet<EventAddressDto>(),
                Responses = new HashSet<EventAddressDto>(),
            };

            //Act
            try
            {
                eventLogic.UpdateEvent(eventDto, new Uri("http://test/")).Wait();
            }
            catch (Exception)
            {
                // ignored
            }


            //Assert
            Assert.AreEqual(true, eventLogic.Included);
            Assert.AreEqual(true, eventLogic.Executed);
            Assert.AreEqual(true, eventLogic.Pending);
            Assert.AreEqual("TestWId", eventLogic.WorkflowId);
            Assert.AreEqual("TestName", eventLogic.Name);
            Assert.AreEqual("TestId", eventLogic.EventId);
            Assert.AreEqual(new HashSet<Uri>(), eventLogic.Inclusions);
            Assert.AreEqual(new HashSet<Uri>(), eventLogic.Exclusions);
            Assert.AreEqual(new HashSet<Uri>(), eventLogic.Conditions);
            Assert.AreEqual(new HashSet<Uri>(), eventLogic.Responses);
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void UpdateEventEventDtoIsNull()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());

            //Act
            try
            {
                eventLogic.UpdateEvent(null, new Uri("http://test/")).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("Provided EventDto was null", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void UpdateEventEventIdIsNull()
        {
            //Arrange
            var eventLogic = new EventLogic(new InMemoryStorage2());
            eventLogic.EventId = null;

            //Act
            try
            {
                eventLogic.UpdateEvent(new EventDto(), new Uri("http://test/")).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("EventId was null", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

      
        #endregion

    }
}
