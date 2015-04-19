using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Event.Exceptions;
using Event.Interfaces;
using Event.Logic;
using Event.Models;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Event.Tests.LogicTests
{
    class LockLogicTests
    {

        #region Setup

        /// <summary>
        /// This method returns a ILockingLogic instance, that was initialized using dependency-injection.
        /// The injected (mocked) modules are not configured; this method should not be used, if you intend on testing
        /// some interaction with either EventCommunicator or EventStorage. 
        /// </summary>
        /// <returns></returns>
        public ILockingLogic SetupDefaultLockingLogic()
        {
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                (IEventStorage) mockStorage.Object,
                (IEventFromEvent) mockEventCommunicator.Object);

            return logic;
        }
        #endregion


        #region IsAllowedToOperate tests

        [Test]
        public void IsAllowedToOperate_ReturnsTrueWhenNoLockIsSet()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>())).Returns(() => Task.Run(() => (LockDto)null));
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                (IEventStorage) mockStorage.Object,
                (IEventFromEvent) mockEventCommunicator.Object);

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
            var lockDto = new LockDto()
            {
                LockOwner = "EventA",
                EventIdentificationModel = null,
                Id = "DatabaseRelevantId"
            };
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>())).Returns(Task.Run(() => lockDto));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                (IEventStorage) mockStorage.Object,
                (IEventFromEvent) mockEventCommunicator.Object);

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
            var lockDto = new LockDto()
            {
                LockOwner = "EventA",       // Notice, EventA is locking!
                EventIdentificationModel = null,
                Id = "DatabaseRelevantId"
            };
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>())).Returns(Task.Run(() => lockDto));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                (IEventStorage)mockStorage.Object,
                (IEventFromEvent)mockEventCommunicator.Object);

            // Act
            var result = logic.IsAllowedToOperate("irrelevantToTestId", "EventB").Result; // Notice EventB is used here

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsAllowedToOperate_RaisesExceptionIfProvidedNullEventId()
        {
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var task = logic.IsAllowedToOperate(null, "EventA");
            
            // Assert
            if (task.Exception == null)
            {
                Assert.Fail("Task should have raised an exception");
            }
            var innerException = task.Exception.InnerException;

            Assert.IsInstanceOf<ArgumentNullException>(innerException);
        }

        [Test]
        public void IsAllowedToOperate_RaisesExceptionIfProvidedNullCallerId()
        {
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var task = logic.IsAllowedToOperate("someEvent", null);

            // Assert
            if (task.Exception == null)
            {
                Assert.Fail("Task should have raised an exception");
            }
            var innerException = task.Exception.InnerException;

            Assert.IsInstanceOf<ArgumentNullException>(innerException);
        }

        #endregion 

        #region LockAll tests
        // TODO: How do we test this?

        [Test]
        public void LockAll_WillRaiseExceptionIfEventIdIsNull()
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var lockAllTask = logic.LockAll(null);

            // Assert
            if (lockAllTask.Exception == null)
            {
                Assert.Fail("lockAllTask was expected to contain a non-null Exception-property");
            }

            var innerException = lockAllTask.Exception.InnerException;

            Assert.IsInstanceOf<ArgumentNullException>(innerException);
        }
        #endregion

        #region LockSelf tests
        [Test]
        public void LockSelf_WillRaiseExceptionIfLockDtoIsNull()
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act 
            var task = logic.LockSelf("testA", null);

            // Assert
            if (task.Exception == null)
            {
                Assert.Fail("Task should have thrown an exception");
            }
            Assert.IsInstanceOf<ArgumentNullException>(task.Exception.InnerException);
        }

        [Test]
        public void LockSelf_WillRaiseExceptionIfEventIdIsNull()
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            var lockDto = new LockDto()
            {
                EventIdentificationModel = new EventIdentificationModel(),
                Id = "DatabaseId",
                LockOwner = "whatever"
            };

            // Act 
            var task = logic.LockSelf(null,lockDto);

            // Assert
            if (task.Exception == null)
            {
                Assert.Fail("Task should have thrown an exception");
            }
            Assert.IsInstanceOf<ArgumentNullException>(task.Exception.InnerException);
        }



        #endregion

        #region UnlockAll tests
        // TODO: How do we do this?

        [Test]
        public void UnlockAll_WillRaiseExceptionIfEventIdWasNull()
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var unlockAllTask = logic.UnlockAll(null);

            // Assert
            if (unlockAllTask.Exception == null)
            {
                Assert.Fail("Task should have thrown an exception");
            }
            Assert.IsInstanceOf<ArgumentNullException>(unlockAllTask.Exception.InnerException);
        }

        [Test]
        public void UnlockAll_WillRaiseExceptionIfStorageReturnsNullRelationsSets()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.GetInclusions(It.IsAny<string>()))
                .Returns(() => (HashSet<RelationToOtherEventModel>) null);

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                (IEventStorage) mockStorage.Object,
                (IEventFromEvent) mockEventCommunicator.Object);

            // Act
            var unlockAllTask = logic.UnlockAll("someEvent");

            // Assert
            if (unlockAllTask.Exception == null)
            {
                Assert.Fail("Task should have thrown an exception");
            }
            Assert.IsInstanceOf<NullReferenceException>(unlockAllTask.Exception.InnerException);
        }
        #endregion

        #region UnlockSelf tests

        [Test]
        public void UnlockSelf_WillRaiseExceptionIfCalledWithNullEventId()
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var unlockSelfTask = logic.UnlockSelf(null, "someEvent");

            // Assert
            if (unlockSelfTask.Exception == null)
            {
                Assert.Fail("Task should have thrown an exception");
            }
            Assert.IsInstanceOf<ArgumentNullException>(unlockSelfTask.Exception.InnerException);
        }

        [Test]
        public void UnlockSelf_WillRaiseExceptionIfCalledWithNullCallerId()
        {
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var unlockSelfTask = logic.UnlockSelf("someEvent", null);

            // Assert
            if (unlockSelfTask.Exception == null)
            {
                Assert.Fail("Task should have thrown an exception");
            }
            Assert.IsInstanceOf<ArgumentNullException>(unlockSelfTask.Exception.InnerException);
        }

        [Test]
        public void UnlockSelf_WillRaiseExceptionIfEventIsLockedBySomeoneElse()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var lockDtoToReturnFromStorage = new LockDto()
            {
                EventIdentificationModel = new EventIdentificationModel(),
                Id = "databaseRelevantId",
                LockOwner = "Johannes"          // Notice, Johannes will be the lockOwner according to Storage!
            };

            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>()))
                .Returns(() => Task.Run(() => lockDtoToReturnFromStorage));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                (IEventStorage) mockStorage.Object,
                (IEventFromEvent) mockEventCommunicator.Object);

            // Act
            var unlockSelfTask = logic.UnlockSelf("irrelevantId", "Per"); // Notice, we're trying to let Per unlock

            // Assert
            if (unlockSelfTask.Exception == null)
            {
                Assert.Fail("Task should have thrown an exception");
            }
            Assert.IsInstanceOf<LockedException>(unlockSelfTask.Exception.InnerException);
        }

        #endregion

        #region UnlockSome tests
        // TODO: What to do? This is a private method? How to test? 
        #endregion

    }
}
