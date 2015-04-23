using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Exceptions;
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
            var eventDto = new EventDto
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
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.Exists(It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.Run(() => true));
           
            var mockResetStorage = new Mock<IEventStorageForReset>();

            var mockLockingLogic = new Mock<ILockingLogic>();

            ILifecycleLogic logic = new LifecycleLogic(
                mockStorage.Object,
                mockResetStorage.Object, 
                mockLockingLogic.Object);

            var eventDto = new EventDto
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
            try
            {
                await logic.CreateEvent(eventDto, uri);
            }
            catch (Exception e)
            {
                // Assert
                Assert.IsInstanceOf<EventExistsException>(e);
            }
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
            ILifecycleLogic logic = new LifecycleLogic(mockStorage.Object, mockResetStorage.Object, mockLockingLogic.Object);

            // If this method should throw an exception, the unit test will fail, hence no need to assert
            logic.DeleteEvent("notWorkflowId", "nonexistingId");
        }

        [Test]
        public void DeleteEvent_WillFailIfEventIsLockedByAnotherEvent()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.Run(() => new LockDto
            {
                LockOwner = "AnotherEventWhoLockedMeId",
                WorkflowId = "workflowId", 
                Id = "AnotherEventWhoLockedMeId"
            }));

            var mockResetStorage = new Mock<IEventStorageForReset>();
            var mockLockingLogic = new Mock<ILockingLogic>();

            ILifecycleLogic logic = new LifecycleLogic(mockStorage.Object,mockResetStorage.Object,mockLockingLogic.Object);

            // Act
            var testDelegate = new TestDelegate(async () => await logic.DeleteEvent("workflowId", "Check patient"));


            // Aseert
            Assert.Throws<LockedException>(testDelegate);
        }
        #endregion

        #region ResetEvent tests

        #endregion

        #region GetEvent tests
        [Test]
        public void GetEvent_Will_Throw_NotFoundException_If_Ids_Does_Not_Exist()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.Exists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            var mockResetStorage = new Mock<IEventStorageForReset>();
            var mockLockLogic = new Mock<ILockingLogic>();

            ILifecycleLogic logic = new LifecycleLogic(mockStorage.Object,mockResetStorage.Object,mockLockLogic.Object);

            // Act
            var testdelegate = new TestDelegate(async () => await logic.GetEventDto("workflowId", "someEvent"));

            // Assert
            Assert.Throws<NotFoundException>(testdelegate);
        }

        [Test]
        public void GetEvent_WillThrowExceptionWhenCalledWithNullEventId()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockResetStorage = new Mock<IEventStorageForReset>();
            var mockLockLogic = new Mock<ILockingLogic>();

            ILifecycleLogic logic = new LifecycleLogic(
                mockStorage.Object,
                mockResetStorage.Object,
                mockLockLogic.Object);

            // Act
            var testDelegate = new TestDelegate(async () => await logic.GetEventDto(null, null));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        #endregion



    }
}
