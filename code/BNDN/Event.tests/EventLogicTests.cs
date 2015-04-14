using System;
using System.Collections.Generic;
using Common;
using Event.Interfaces;
using Event.Logic;
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
            mock.Setup(storage => storage.GetIncluded(It.IsAny<string>())).Returns(true);
            mock.Setup(storage => storage.GetConditions(It.IsAny<string>())).Returns(new HashSet<RelationToOtherEventModel>());

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
            mock.Setup(storage => storage.GetIncluded(It.IsAny<string>())).Returns(false);

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
            var storage = new Mock<IEventStorage>();
            storage.Setup(s => s.GetIncluded(It.IsAny<string>())).Returns(false);
            storage.Setup(s => s.GetExecuted(It.IsAny<string>())).Returns(true);
            storage.Setup(s => s.GetPending(It.IsAny<string>())).Returns(true);

            var locklogic = new Mock<ILockingLogic>();

            var stateLogic = new StateLogic(storage.Object, locklogic.Object, new AuthLogic(storage.Object));

            //Act
            var result = await stateLogic.GetStateDto("eventId", "-1");

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
            mock.Setup(storage => storage.GetIncluded(It.IsAny<string>())).Returns(true);
            mock.Setup(storage => storage.GetExecuted(It.IsAny<string>())).Returns(false);
            mock.Setup(storage => storage.GetPending(It.IsAny<string>())).Returns(false);
            mock.Setup(storage => storage.GetConditions(It.IsAny<string>())).Returns(new HashSet<RelationToOtherEventModel>());

            var eventLogic = new EventLogic(mock.Object);
          
            //Act
            var result = await eventLogic.GetEventStateDto();

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
            var result = eventLogic.GetEventDto();

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitializeEventEventDtoIsNull()
        {
            //Arrange
            var lifecycleLogic = new LifecycleLogic(Mock.Of<IEventStorage>());

            //Act
            try
            {
                lifecycleLogic.CreateEvent(null, new Uri("http://test/")).Wait();
            }
            catch (Exception ex)
            {
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

            var lifecycleLogic = new LifecycleLogic(mock.Object);

            //Act
            try
            {
                await lifecycleLogic.CreateEvent(new EventDto(), new Uri("http://test/"));
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("EventId was not null", ex.InnerException.Message);
                throw ex.InnerException;
            }
        }
        #endregion

    }
}
