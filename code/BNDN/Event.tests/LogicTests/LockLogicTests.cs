using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                mockStorage.Object,
                mockEventCommunicator.Object);
            
            return logic;
        }
        #endregion

        #region AddToQueue Tests

        [Test]
        public void AddToQueue_ElementGetsAdded_EmptyQueue()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            LockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            LockDto lockDto = new LockDto { Id = "LockId", LockOwner = "lockOwner", WorkflowId = "Wid" };
            // Act
            logic.AddToQueue("Wid","Eid",lockDto);
            
            // Assert
            Assert.Equals(1, LockingLogic.LockQueue["Wid"]["Eid"].Count);

            LockDto result;
            LockingLogic.LockQueue["Wid"]["Eid"].TryDequeue(out result);
            Assert.Equals(lockDto, result);

            Assert.IsEmpty(LockingLogic.LockQueue["Wid"]["Eid"]);
            // Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [Test]
        public void AddToQueue_ElementGetsAdded_1ElementInQueue()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            LockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);
            LockingLogic.LockQueue["Wid"]["Eid"].Enqueue(new LockDto{Id = "AlreadyThereId"});

            LockDto lockDto = new LockDto { Id = "LockId", LockOwner = "lockOwner", WorkflowId = "Wid" };
            // Act
            logic.AddToQueue("Wid", "Eid", lockDto);

            // Assert
            LockDto result;
            LockingLogic.LockQueue["Wid"]["Eid"].TryDequeue(out result);
            Assert.AreNotEqual(lockDto, result);

            LockingLogic.LockQueue["Wid"]["Eid"].TryDequeue(out result);
            Assert.Equals(lockDto, result);

            Assert.IsEmpty(LockingLogic.LockQueue["Wid"]["Eid"]);
            // Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [Test]
        public void AddToQueue_QueueSize1UpAfterRun()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            LockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            LockDto lockDto = new LockDto { Id = "LockId", LockOwner = "lockOwner", WorkflowId = "Wid" };

            var previousSize = LockingLogic.LockQueue["Wid"]["Eid"].Count;
            // Act
            logic.AddToQueue("Wid", "Eid", lockDto);
            // Assert
            Assert.Equals(previousSize + 1, LockingLogic.LockQueue["Wid"]["Eid"].Count);
            // Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [Test]
        public void AddToQueue_ElementIsNull()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            LockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            LockDto lockDto = null;
            // Act and Assert

            Assert.Throws<Exception>(() =>{ logic.AddToQueue("Wid", "Eid", lockDto); });

            // Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [Test]
        public void AddToQueue_ElementAlreadyExists()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            LockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            LockDto lockDto = new LockDto { Id = "LockId", LockOwner = "lockOwner", WorkflowId = "Wid" };
            // Act and Assert

            logic.AddToQueue("Wid", "Eid", lockDto);
            Assert.Throws<Exception>(() => { logic.AddToQueue("Wid", "Eid", lockDto); });

            // Cleanup
            LockingLogic.LockQueue.Clear();
        }
        #endregion


        #region IsAllowedToOperate tests

        [Test]
        public void IsAllowedToOperate_ReturnsTrueWhenNoLockIsSet()
        {
            // Arrange
            var mockStorage = new Mock<IEventStorage>();
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>(), It.IsAny<string>())).Returns(() => Task.Run(() => (LockDto)null));
            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var result = logic.IsAllowedToOperate("workflowId", "testA", "testB").Result;

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
                WorkflowId = "workflowId",
                Id = "DatabaseRelevantId"
            };
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.Run(() => lockDto));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var result = logic.IsAllowedToOperate("workflowId", "irrelevantToTestId", "EventA").Result;

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
                WorkflowId = "workflowId", 
                Id = "DatabaseRelevantId"
            };
            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.Run(() => lockDto));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var result = logic.IsAllowedToOperate("workflowId", "irrelevantToTestId", "EventB").Result; // Notice EventB is used here

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsAllowedToOperate_RaisesExceptionIfProvidedNullEventId()
        {
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var testDelegate = new TestDelegate(async () => await logic.IsAllowedToOperate(null, null, "EventA"));
            
            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void IsAllowedToOperate_RaisesExceptionIfProvidedNullCallerId()
        {
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var testDelegate = new TestDelegate(async () => await logic.IsAllowedToOperate("workflowId", "someEvent", null));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        #endregion 

        #region LockAllForExecute tests

        [Test]
        public void LockAll_WillRaiseExceptionIfEventIdIsNull()
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var lockAllTask = logic.LockAllForExecute(null, null);

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
            LockDto nullLockDto = null;
            var testDelegate = new TestDelegate(async () => await logic.LockSelf("workflowId", "testA", nullLockDto));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void LockSelf_WillRaiseExceptionIfEventIdIsNull()
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            var lockDto = new LockDto
            {
                WorkflowId = "workflowId", 
                Id = "DatabaseId",
                LockOwner = "whatever"
            };

            // Act 
            var testDelegate = new TestDelegate(async () => await logic.LockSelf(null, null,lockDto));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }



        #endregion

        #region UnlockAllForExecute tests

        [Test]
        public void UnlockAll_WillRaiseExceptionIfEventIdWasNull()
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var unlockAllTask = logic.UnlockAllForExecute(null, null);

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
            mockStorage.Setup(m => m.GetInclusions(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.Run(() => (HashSet<RelationToOtherEventModel>) null));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var unlockAllTask = logic.UnlockAllForExecute("workflowId", "someEvent");

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
            var unlockSelfTask = logic.UnlockSelf(null, null, "someEvent");

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
            var unlockSelfTask = logic.UnlockSelf("workflowId", "someEvent", null);

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
            var lockDtoToReturnFromStorage = new LockDto
            {
                WorkflowId = "workflowId", 
                Id = "databaseRelevantId",
                LockOwner = "Johannes"          // Notice, Johannes will be the lockOwner according to Storage!
            };

            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(lockDtoToReturnFromStorage);

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic logic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            // Act
            var testDelegate = new TestDelegate(async () => await logic.UnlockSelf("workflowId", "irrelevantId", "Per")); // Notice, we're trying to let Per unlock

            // Assert
            Assert.Throws<LockedException>(testDelegate);
        }

        #endregion

        #region UnlockSome tests
        // dont test private methods.
        #endregion

    }
}
