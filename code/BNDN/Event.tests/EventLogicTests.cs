using System;
using System.Collections.Generic;
using System.Web.Http;
using Common;
using Event.Interfaces;
using Event.Models;
using Event.Storage;
using Moq;
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
            var eventLogic = new EventLogic(Mock.Of<IEventStorage>());//new InMemoryStorage2());

            //Act

            //Assert
            Assert.IsNotNull(eventLogic);
        }


        #region Rulehandling
        [Test]
        public void IsExecutable_ShouldReturnTrueWhenNoConditionsExist_Test()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.Setup(storage => storage.Included).Returns(true);
            mock.Setup(storage => storage.Conditions).Returns(new HashSet<RelationToOtherEventModel>());

            var eventLogic = new EventLogic(mock.Object);

            //Act
            var result = eventLogic.IsExecutable().Result;

            //Assert
            Assert.AreEqual(true, result);
        }
        [Test]
        public void IsExecutable_ShouldReturnFalseWhenNoConditionsExist_Test()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.Setup(storage => storage.Included).Returns(false);

            var eventLogic = new EventLogic(mock.Object);

            //Act
            var result = eventLogic.IsExecutable().Result;

            //Assert
            Assert.AreEqual(false, result);
        }

        /*
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
        */
        #endregion

        #region DTO Creation

        [Test]
        public async void GetEventStateDtoWhenIncludedIsFalseTest()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.Setup(storage => storage.Included).Returns(false);
            mock.Setup(storage => storage.Executed).Returns(true);
            mock.Setup(storage => storage.Pending).Returns(true);

            var eventLogic = new EventLogic(mock.Object);

            //Act
            var result = await eventLogic.EventStateDto;

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
        public async void GetEventStateDtoWhenIncludedIsTrueTest()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.Setup(storage => storage.Included).Returns(true);
            mock.Setup(storage => storage.Executed).Returns(false);
            mock.Setup(storage => storage.Pending).Returns(false);
            mock.Setup(storage => storage.Conditions).Returns(new HashSet<RelationToOtherEventModel>());

            var eventLogic = new EventLogic(mock.Object);
          
            //Act
            var result = await eventLogic.EventStateDto;

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
        public async void GetEventDtoPropertyTest()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.SetupAllProperties();

            var eventLogic = new EventLogic(mock.Object);
            eventLogic.Included = true;
            eventLogic.Executed = false;
            eventLogic.Pending = false;
            eventLogic.EventId = "TestId";
            eventLogic.Name = "TestName";
            eventLogic.WorkflowId = "TestWId";
            eventLogic.Conditions = new HashSet<RelationToOtherEventModel>();
            eventLogic.Inclusions = new HashSet<RelationToOtherEventModel>();
            eventLogic.Exclusions = new HashSet<RelationToOtherEventModel>();
            eventLogic.Responses = new HashSet<RelationToOtherEventModel>();


            //Act
            var result = await eventLogic.EventDto;

            //Assert
            Assert.AreEqual(true, result.Included);
            Assert.AreEqual(false, result.Executed);
            Assert.AreEqual(false, result.Pending);
            Assert.AreEqual("TestWId", result.WorkflowId);
            Assert.AreEqual("TestName", result.Name);
            Assert.AreEqual("TestId", result.EventId);
        }


        #region URI registration

        /* Todo: Redesign this test to use the new way of deleting events.
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
        }*/

        [Test]
        //This test only tests that values are set, and not a connection to the server is established.
        public async void InitializeEventRuns()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.SetupAllProperties();
            mock.Setup(storage => storage.EventId).Returns("TestId");

            var eventLogic = new EventLogic(mock.Object);
            var eventDto = new EventDto
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
                await eventLogic.InitializeEvent(eventDto, new Uri("http://test/"));
            }
            catch (HttpResponseException)
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitializeEventEventDtoIsNull()
        {
            //Arrange
            var eventLogic = new EventLogic(Mock.Of<IEventStorage>());

            //Act
            try
            {
                eventLogic.InitializeEvent(null, new Uri("http://test/")).Wait();
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("Provided EventDto was null\r\nParameternavn: eventDto", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public async void InitializeEventEventIdIsNotNull()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.SetupAllProperties();

            var eventLogic = new EventLogic(mock.Object);
            eventLogic.EventId = "Test";

            //Act
            try
            {
                await eventLogic.InitializeEvent(new EventDto(), new Uri("http://test/"));
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
        public async void UpdateEventRuns()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.SetupAllProperties();
            mock.Setup(storage => storage.EventId).Returns("TestId");

            var eventLogic = new EventLogic(mock.Object);
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
                await eventLogic.UpdateEvent(eventDto, new Uri("http://test/"));
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
        public async void UpdateEventEventDtoIsNull()
        {
            //Arrange
            var eventLogic = new EventLogic(Mock.Of<IEventStorage>());

            //Act
            try
            {
                await eventLogic.UpdateEvent(null, new Uri("http://test/"));
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
        public async void UpdateEventEventIdIsNull()
        {
            //Arrange
            var mock = new Mock<IEventStorage>();
            mock.SetupAllProperties();

            var eventLogic = new EventLogic(mock.Object);
            eventLogic.EventId = null;

            //Act
            try
            {
                await eventLogic.UpdateEvent(new EventDto(), new Uri("http://test/"));
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
