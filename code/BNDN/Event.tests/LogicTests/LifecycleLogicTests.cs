using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Exceptions;
using Event.Interfaces;
using Event.Logic;
using Event.Models;
using Moq;
using NUnit.Framework;

namespace Event.Tests.LogicTests
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
        public void CreateEvent_WithIdAlreadyInDatabase()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.Exists(It.IsAny<string>())).Returns(() => Task.Run(() => true));
           
            var mockResetStorage = new Mock<IEventStorageForReset>();

            var mockLockingLogic = new Mock<ILockingLogic>();

            ILifecycleLogic logic = new LifecycleLogic(
                (IEventStorage) mockStorage.Object,
                (IEventStorageForReset) mockResetStorage.Object, 
                (ILockingLogic) mockLockingLogic.Object);

            var eventDto = new EventDto()
            {
                Conditions = new List<EventAddressDto>(),
                EventId = "theAwesomeEventId",
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

            var uri = new Uri("http://www.dr.dk");

            // Act
            var createTask = logic.CreateEvent(eventDto, uri);
            var exception = createTask.Exception.InnerException;

            // Assert
            Assert.IsInstanceOf<ApplicationException>(exception);
        }

        /*public void CreateEvent_CreatesEventOnStorage()
        {
            
        }*/
        #endregion

        #region DeleteEvent tests

        /*[Test]
        public void DeleteEvent_CalledWithEmptyStringWillNotFail()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockResetStorage = new Mock<IEventStorageForReset>();
            var mockLockingLogic = new Mock<ILockingLogic>();
            mockLockingLogic.Setup(m => m.IsAllowedToOperate(It.IsAny<string>()))
            ILifecycleLogic logic = new LifecycleLogic((IEventStorage)mockStorage.Object, (IEventStorageForReset)mockResetStorage.Object, (ILockingLogic)mockLockingLogic.Object);

            var task = logic.DeleteEvent("nonexistingId");

            Assert.IsNull(task.Exception);          // "Task.Exception:  If the Task completed successfully or has not yet thrown any exceptions, this will return null."
        }*/

        [Test]
        public void DeleteEvent_DeleteNonExistingIdDoesNotThrowException()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockResetStorage = new Mock<IEventStorageForReset>();
            var mockLockingLogic = new Mock<ILockingLogic>();
            ILifecycleLogic logic = new LifecycleLogic((IEventStorage)mockStorage.Object,(IEventStorageForReset)mockResetStorage.Object,(ILockingLogic) mockLockingLogic.Object);

            // If this method should throw an exception, the unit test will fail, hence no need to assert
            logic.DeleteEvent("nonexistingId");
        }

        [Test]
        public void DeleteEvent_WillFailIfEventIsLockedByAnotherEvent()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>())).Returns(Task.Run(() => new LockDto()
            {
                LockOwner = "AnotherEventWhoLockedMeId",
                EventIdentificationModel = new EventIdentificationModel(),
                Id = "AnotherEventWhoLockedMeId"
            }));

            var mockResetStorage = new Mock<IEventStorageForReset>();
            var mockLockingLogic = new Mock<ILockingLogic>();

            ILifecycleLogic logic = new LifecycleLogic((IEventStorage) mockStorage.Object,(IEventStorageForReset) mockResetStorage.Object,(ILockingLogic) mockLockingLogic.Object);

            var deleteEventTask = logic.DeleteEvent("Check patient");

            var exception = deleteEventTask.Exception.InnerException;

            // Aseert
            Assert.IsInstanceOf<LockedException>(exception);
        }
        #endregion

        #region ResetEvent tests

        #endregion

        #region GetEvent tests
        [Test]
        public void GetEvent_WillReturnNullIfNoMatchingEventExists()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.Exists(It.IsAny<string>())).Returns(() => Task.Run(() => false));
            var mockResetStorage = new Mock<IEventStorageForReset>();
            var mockLockLogic = new Mock<ILockingLogic>();

            ILifecycleLogic logic = new LifecycleLogic((IEventStorage) mockStorage.Object,(IEventStorageForReset) mockResetStorage.Object,(ILockingLogic) mockLockLogic.Object);

            // Act
            var getEvent = logic.GetEventDto("someEvent").Result;

            // Assert
            Assert.IsNull(getEvent);

        }

        [Test]
        public void GetEvent_WillThrowExceptionWhenCalledWithNullEventId()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockResetStorage = new Mock<IEventStorageForReset>();
            var mockLockLogic = new Mock<ILockingLogic>();

            ILifecycleLogic logic = new LifecycleLogic(
                (IEventStorage) mockStorage.Object,
                (IEventStorageForReset) mockResetStorage.Object,
                (ILockingLogic) mockLockLogic.Object);

            // Act
            var task = logic.GetEventDto(null);

            // Assert
            if (task.Exception == null)
            {
                Assert.Fail("task was expected to return an exception");
            }

            Assert.IsInstanceOf<ArgumentNullException>(task.Exception.InnerException);
        }

        #endregion



    }
}
