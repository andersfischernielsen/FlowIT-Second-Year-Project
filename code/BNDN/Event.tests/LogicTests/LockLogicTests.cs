using System;
using System.Threading.Tasks;
using Event.Interfaces;
using Event.Logic;
using Event.Models;
using Moq;
using NUnit.Framework;

namespace Event.Tests.LogicTests
{
    [TestFixture]
    class LockLogicTests
    {


        #region IsAllowedToOperate tests

        [Test]
        public void IsAllowedToOperate_ReturnsTrueWhenNoLockIsSet()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>())).Returns(() => Task.Run(() => (LockDto)null));
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var result = logic.IsAllowedToOperate("testA", "testB").Result;

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsAllowedToOperate_ReturnsTrueWhenEventWasPreviouslyLockedWithCallersId()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var lockDto = new LockDto
            {
                LockOwner = "EventA",
                EventIdentificationModel = null,
                Id = "DatabaseRelevantId"
            };
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>())).Returns(Task.Run(() => lockDto));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var result = logic.IsAllowedToOperate("irrelevantToTestId", "EventA").Result;

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsAllowedToOperate_ReturnsFalseWhenEventWasPreviouslyLockedWithAnotherId()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var lockDto = new LockDto
            {
                LockOwner = "EventA",       // Notice, EventA is locking!
                EventIdentificationModel = null,
                Id = "DatabaseRelevantId"
            };
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>())).Returns(Task.Run(() => lockDto));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var result = logic.IsAllowedToOperate("irrelevantToTestId", "EventB").Result; // Notice EventB is used here

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsAllowedToOperate_RaisesExceptionIfProvidedNullEventId()
        {
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            var logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var testDelegate = new TestDelegate(async () => await logic.IsAllowedToOperate(null, "EventA"));
            
            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void IsAllowedToOperate_RaisesExceptionIfProvidedNullCallerId()
        {
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            var logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var testDelegate = new TestDelegate(async () => await logic.IsAllowedToOperate("someEvent", null));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        #endregion 

        #region LockAll tests
        #endregion

        #region LockSelf tests
        [Test]
        public void LockSelf_WillRaiseExceptionIfLockDtoIsNull()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);


            // Act 
            var testDelegate = new TestDelegate(async () => await logic.LockSelf("testA", null));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void LockSelf_WillRaiseExceptionIfEventIdIsNull()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            var lockDto = new LockDto
            {
                EventIdentificationModel = new EventIdentificationModel(),
                Id = "DatabaseId",
                LockOwner = "whatever"
            };

            // Act 
            var testDelegate = new TestDelegate(async () => await logic.LockSelf(null,lockDto));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }



        #endregion

        #region UnlockAll tests

        #endregion

        #region UnlockSelf tests

        #endregion

        #region UnlockSome tests

        #endregion

    }
}
