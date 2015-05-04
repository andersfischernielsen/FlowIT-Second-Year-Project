using System;
using System.Collections.Concurrent;
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


        #region WaitForMyTurn Tests

        [TestCase("AnotherWid","Eid")]
        [TestCase("Wid", "AnotherEid")]
        [TestCase("AnotherWid", "AnotherEid")]
        public async void WaitForMyTurn_Succes_OtherLocksOnOtherWorkflowsExist(string alreadyWid, string alreadyEid)
        {
            //Arrange
            ILockingLogic lockingLogic = SetupDefaultLockingLogic();

            string eventId = "Eid";
            string workflowId = "Wid";

            var eventDictionary = LockingLogic.LockQueue.GetOrAdd(alreadyWid, new ConcurrentDictionary<string, ConcurrentQueue<LockDto>>());
            var queue = eventDictionary.GetOrAdd(alreadyEid, new ConcurrentQueue<LockDto>());
            queue.Enqueue(new LockDto { WorkflowId = alreadyWid, EventId = alreadyEid, LockOwner = "AlreadyThereOwner" });

            LockDto lockDto = new LockDto { EventId = eventId, LockOwner = "LockOwner", WorkflowId = workflowId };
            //Act
            await lockingLogic.WaitForMyTurn(workflowId, eventId, lockDto);
            //Assert
            Assert.IsEmpty(LockingLogic.LockQueue[workflowId][eventId]);
            //Cleanup
            LockingLogic.LockQueue.Clear();
        }
        
        [Test]
        public async void WaitForMyTurn_Succes_EmptyQueueLockEntersAndLeaves()
        {
            //Arrange
            ILockingLogic lockingLogic = SetupDefaultLockingLogic();
            LockDto lockDto = new LockDto { EventId = "Eid", LockOwner = "LockOwner", WorkflowId = "Wid" };
            //Act
            await lockingLogic.WaitForMyTurn("Wid", "Eid", lockDto);
            //Assert
            Assert.IsEmpty(LockingLogic.LockQueue["Wid"]["Eid"]);
            //Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [Test]
        public async void WaitForMyTurn_Succes_QueueHasAnElementWhichGetsRemovedAfter5Seconds()
        {
            //Arrange
            var mockStorage = new Mock<IEventStorage>();
            var lockDtoToReturnFromStorage = new LockDto
            {
                WorkflowId = "Wid",
                EventId = "Eid",
                LockOwner = "AlreadyThereOwner"          // Notice, AlreadyThereOwner will be the lockOwner according to Storage!
            };

            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(lockDtoToReturnFromStorage);

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic lockingLogic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            string eventId = "Eid";
            string workflowId = "Wid";

            var eventDictionary = LockingLogic.LockQueue.GetOrAdd(workflowId, new ConcurrentDictionary<string, ConcurrentQueue<LockDto>>());
            var queue = eventDictionary.GetOrAdd(eventId, new ConcurrentQueue<LockDto>());
            queue.Enqueue(new LockDto { WorkflowId = workflowId, EventId = eventId, LockOwner = "AlreadyThereOwner" });

            LockDto lockDto = new LockDto { EventId = eventId, LockOwner = "LockOwner", WorkflowId = workflowId };
            //Act
            //DO NOT AWAIT
            var task = Task.Run(async () =>
            {
                await Task.Delay(5000);
                LockDto dequeuedDto;
                queue.TryDequeue(out dequeuedDto);
            });
            // To begin with we want the task to still be running which removes the queued object
            if (!task.IsCompleted)
            {
                await lockingLogic.WaitForMyTurn("Wid", "Eid", lockDto);
                // after waiting for my turn, the task must have been completed.
                if(!task.IsCompleted) Assert.Fail();
            }
            else
            {
                Assert.Fail();
            }
            //Assert
            Assert.IsEmpty(LockingLogic.LockQueue["Wid"]["Eid"]);
            //Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [Test]
        public async void WaitForMyTurn_Succes_AlreadyLockedBySelf()
        {
            //Arrange
            string eventId = "Eid";
            string workflowId = "Wid";
            string lockOwner = "lockOwner";

            var mockStorage = new Mock<IEventStorage>();
            var lockDtoToReturnFromStorage = new LockDto
            {
                WorkflowId = workflowId,
                EventId = eventId,
                LockOwner = lockOwner
            };

            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(lockDtoToReturnFromStorage);

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic lockingLogic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            LockDto lockDto = new LockDto { EventId = eventId, LockOwner = lockOwner, WorkflowId = workflowId };
            //Act
            await lockingLogic.WaitForMyTurn(workflowId, eventId, lockDto);
            //Assert
            Assert.IsEmpty(LockingLogic.LockQueue[workflowId][eventId]);
            //Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [TestCase(null,"")]
        [TestCase("", null)]
        [TestCase(null, null)]
        public async void WaitForMyTurn_ParameterIsNull(string workflowId, string eventId)
        {
            //Arrange
            ILockingLogic lockingLogic = SetupDefaultLockingLogic();
            LockDto lockDto = new LockDto{ EventId = eventId, LockOwner = "LockOwner", WorkflowId = workflowId};
            //Act
            var testDelegate = new TestDelegate(async () => await lockingLogic.WaitForMyTurn(workflowId, eventId, lockDto));
            //Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
            //Cleanup
            LockingLogic.LockQueue.Clear();
        }
        [Test]
        public async void WaitForMyTurn_LockDtoIsNull()
        {
            //Arrange
            ILockingLogic lockingLogic = SetupDefaultLockingLogic();
            LockDto lockDto = null;
            //Act
            var testDelegate = new TestDelegate(async () => await lockingLogic.WaitForMyTurn("Wid", "Eid", lockDto));
            //Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
            //Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\n")]
        [TestCase("\t")]
        [TestCase(null)]
        public async void WaitForMyTurn_LockDtoOwnerIsNull(string lockOwner)
        {
            //Arrange
            ILockingLogic lockingLogic = SetupDefaultLockingLogic();
            LockDto lockDto = new LockDto { EventId = "Eid", LockOwner = lockOwner, WorkflowId = "Wid" };
            //Act
            var testDelegate = new TestDelegate(async () => await lockingLogic.WaitForMyTurn("Wid", "Eid", lockDto));
            //Assert
            Assert.Throws<ArgumentException>(testDelegate);
            //Cleanup
            LockingLogic.LockQueue.Clear();
        }

        [Test]
        public async void WaitForMyTurn_QueueHasAnElementWhichDoesNotGetRemoved()
        {
            //Arrange
            var mockStorage = new Mock<IEventStorage>();
            var lockDtoToReturnFromStorage = new LockDto
            {
                WorkflowId = "Wid",
                EventId = "Eid",
                LockOwner = "AlreadyThereOwner"          // Notice, AlreadyThereOwner will be the lockOwner according to Storage!
            };

            mockStorage.Setup(m => m.GetLockDto(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(lockDtoToReturnFromStorage);

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic lockingLogic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            string eventId = "Eid";
            string workflowId = "Wid";

            var eventDictionary = LockingLogic.LockQueue.GetOrAdd(workflowId, new ConcurrentDictionary<string, ConcurrentQueue<LockDto>>());
            var queue = eventDictionary.GetOrAdd(eventId, new ConcurrentQueue<LockDto>());
            queue.Enqueue(new LockDto { WorkflowId = workflowId, EventId = eventId, LockOwner = "AlreadyThereOwner"});

            LockDto lockDto = new LockDto { EventId = eventId, LockOwner = "LockOwner", WorkflowId = workflowId };
            //Act
            var testDelegate = new TestDelegate(async () => await lockingLogic.WaitForMyTurn(workflowId, eventId, lockDto));
            //Assert
            Assert.Throws<LockedException>(testDelegate);
            //Cleanup
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
                EventId = "DatabaseRelevantId"
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
                EventId = "DatabaseRelevantId"
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

        [TestCase(null,null)]
        [TestCase("", null)]
        [TestCase(null, "")]
        [TestCase("text", null)]
        [TestCase(null, "text")]
        public void LockAll_WillRaiseExceptionIfEventIdIsNull(string workflowId, string eventId)
        {
            // Arrange
            ILockingLogic logic = SetupDefaultLockingLogic();

            // Act
            var lockAllTask = logic.LockAllForExecute(workflowId, eventId);

            // Assert
            if (lockAllTask.Exception == null)
            {
                Assert.Fail("lockAllTask was expected to contain a non-null Exception-property");
            }

            var innerException = lockAllTask.Exception.InnerException;

            Assert.IsInstanceOf<ArgumentNullException>(innerException);
        }

        [Test]
        public async void LockAll_Success_EmptyRelationLists()
        {
            //Arrange
            var mockStorage = new Mock<IEventStorage>();

            mockStorage.Setup(m => m.GetConditions(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HashSet<RelationToOtherEventModel>());
            mockStorage.Setup(m => m.GetExclusions(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HashSet<RelationToOtherEventModel>());
            mockStorage.Setup(m => m.GetResponses(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HashSet<RelationToOtherEventModel>());
            mockStorage.Setup(m => m.GetInclusions(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HashSet<RelationToOtherEventModel>());
            mockStorage.Setup(m => m.GetUri(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Uri("http://www.google.com"));

            var mockEventCommunicator = new Mock<IEventFromEvent>();

            ILockingLogic lockingLogic = new LockingLogic(
                mockStorage.Object,
                mockEventCommunicator.Object);

            //Act
            var returnValue = await lockingLogic.LockAllForExecute("Wid","Eid");
            
            //Assert
            
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
                EventId = "DatabaseId",
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
                EventId = "databaseRelevantId",
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
