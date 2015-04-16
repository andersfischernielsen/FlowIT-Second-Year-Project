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

namespace Event.tests
{
    [TestFixture]
    class StateLogicTests
    {
        private StateLogic _stateLogic;

        private Mock<IEventStorage> _eventStorageMock;
        private Mock<ILockingLogic> _lockingLogicMock;
        private Mock<IAuthLogic> _authLogicMock;
        
        [SetUp]
        public void SetUp()
        {
            _eventStorageMock = new Mock<IEventStorage>();

            _eventStorageMock.Setup(s => s.GetIncluded(It.IsAny<string>())).ReturnsAsync(true);
            _eventStorageMock.Setup(s => s.GetConditions(It.IsAny<string>())).Returns(new HashSet<RelationToOtherEventModel>());
            _eventStorageMock.Setup(s => s.GetResponses(It.IsAny<string>())).Returns(new HashSet<RelationToOtherEventModel>());
            _eventStorageMock.Setup(s => s.GetInclusions(It.IsAny<string>())).Returns(new HashSet<RelationToOtherEventModel>());
            _eventStorageMock.Setup(s => s.GetExclusions(It.IsAny<string>())).Returns(new HashSet<RelationToOtherEventModel>());

            _lockingLogicMock = new Mock<ILockingLogic>();

            // Make the Event unlocked unless other is specified.
            _lockingLogicMock.Setup(l => l.IsAllowedToOperate(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _lockingLogicMock.Setup(l => l.LockAll(It.IsAny<string>())).ReturnsAsync(true);
            _lockingLogicMock.Setup(l => l.UnlockAll(It.IsAny<string>())).ReturnsAsync(true);

            _authLogicMock = new Mock<IAuthLogic>();

            // Make the caller authorized
            _authLogicMock.Setup(a => a.IsAuthorized(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(true);

            _stateLogic = new StateLogic(_eventStorageMock.Object, _lockingLogicMock.Object, _authLogicMock.Object);
        }

        [Test]
        public async Task IsExecuted_ReturnsTrue()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetExecuted("eventId")).ReturnsAsync(true);

            // Assert
            Assert.IsTrue(await _stateLogic.IsExecuted("eventId", "senderId"));
        }

        [Test]
        public async Task IsExecuted_ReturnsFalse()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetExecuted("eventId")).ReturnsAsync(false);

            // Assert
            Assert.IsFalse(await _stateLogic.IsExecuted("eventId", "senderId"));
        }

        [Test]
        public void IsExecuted_Throws_LockedException()
        {
            // Arrange
            _lockingLogicMock.Setup(l => l.IsAllowedToOperate("eventId", "senderId")).ReturnsAsync(false);

            // Assert
            Assert.Throws<LockedException>(async () => await _stateLogic.IsExecuted("eventId", "senderId"));
        }

        public async Task IsIncluded_ReturnsTrue()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetIncluded("eventId")).ReturnsAsync(true);

            // Assert
            Assert.IsTrue(await _stateLogic.IsIncluded("eventId", "senderId"));
        }

        public async Task IsIncluded_ReturnsFalse()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetIncluded("eventId")).ReturnsAsync(false);

            // Assert
            Assert.IsFalse(await _stateLogic.IsIncluded("eventId", "senderId"));
        }

        public void IsIncluded_Throws_LockedException()
        {
            // Arrange
            _lockingLogicMock.Setup(l => l.IsAllowedToOperate("eventId", "senderId")).ReturnsAsync(false);

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.IsIncluded("eventId", "senderId"));

            // Assert
            Assert.Throws<LockedException>(testDelegate);
        }

        [Test]
        public async Task GetStateDto_Returns_Executable_State()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetExecuted("eventId")).ReturnsAsync(false);
            _eventStorageMock.Setup(s => s.GetIncluded("eventId")).ReturnsAsync(true);
            _eventStorageMock.Setup(s => s.GetPending("eventId")).ReturnsAsync(false);
            _eventStorageMock.Setup(s => s.GetName("eventId")).ReturnsAsync("Event Name");
            _eventStorageMock.Setup(s => s.GetConditions("eventId")).Returns(new HashSet<RelationToOtherEventModel>());

            // Act
            var result = await _stateLogic.GetStateDto("eventId", "senderId");

            // Assert
            Assert.IsFalse(result.Executed);
            Assert.IsTrue(result.Included);
            Assert.IsFalse(result.Pending);
            Assert.IsTrue(result.Executable);
            Assert.AreEqual("Event Name", result.Name);
            Assert.AreEqual("eventId", result.Id);
        }

        [Test]
        public async Task GetStateDto_Returns_NonExecutable_State()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetExecuted("eventId")).ReturnsAsync(false);
            _eventStorageMock.Setup(s => s.GetIncluded("eventId")).ReturnsAsync(false);
            _eventStorageMock.Setup(s => s.GetPending("eventId")).ReturnsAsync(false);
            _eventStorageMock.Setup(s => s.GetName("eventId")).ReturnsAsync("Event Name");
            _eventStorageMock.Setup(s => s.GetConditions("eventId")).Returns(new HashSet<RelationToOtherEventModel>());

            // Act
            var result = await _stateLogic.GetStateDto("eventId", "senderId");

            // Assert
            Assert.IsFalse(result.Executed);
            Assert.IsFalse(result.Included);
            Assert.IsFalse(result.Pending);
            Assert.IsFalse(result.Executable);
            Assert.AreEqual("Event Name", result.Name);
            Assert.AreEqual("eventId", result.Id);
        }

        [Test]
        public void GetStateDto_Throws_LockedException()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetExecuted("eventId")).ReturnsAsync(false);
            _eventStorageMock.Setup(s => s.GetIncluded("eventId")).ReturnsAsync(true);
            _eventStorageMock.Setup(s => s.GetPending("eventId")).ReturnsAsync(false);
            _eventStorageMock.Setup(s => s.GetName("eventId")).ReturnsAsync("Event Name");
            _eventStorageMock.Setup(s => s.GetConditions("eventId")).Returns(new HashSet<RelationToOtherEventModel>());

            // Make the event locked.
            _lockingLogicMock.Setup(l => l.IsAllowedToOperate("eventId", "senderId")).ReturnsAsync(false);

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.GetStateDto("eventId", "senderId"));

            // Assert
            Assert.Throws<LockedException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_LockedException_When_Another_Event_Has_Lock()
        {
            // Arrange
            // Lock the event to another id.
            _lockingLogicMock.Setup(l => l.IsAllowedToOperate("eventId", "senderId")).ReturnsAsync(false);

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.GetStateDto("eventId", "senderId"));

            // Throws
            Assert.Throws<LockedException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_NotAuthorizedException_When_Role_Is_Wrong()
        {
            // Arrange
            // Make the role wrong:
            _authLogicMock.Setup(a => a.IsAuthorized(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(false);

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.Execute("eventId", new ExecuteDto { Roles = new List<string>{ "WrongRole"}}));

            // Assert
            Assert.Throws<NotAuthorizedException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_NotExecutableException_When_Not_Executable()
        {
            // Arrange
            // Make event not executable:
            _eventStorageMock.Setup(s => s.GetIncluded(It.IsAny<string>())).ReturnsAsync(false);
            // But allow the role:
            _authLogicMock.Setup(a => a.IsAuthorized(It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(true);

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.Execute("eventId", new ExecuteDto{ Roles = new List<string>{ "RightRole"}}));

            // Assert
            Assert.Throws<NotExecutableException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_FailedToLockOtherEventsException_When_Other_Events_Cannot_Be_Locked()
        {
            // Arrange
            _lockingLogicMock.Setup(l => l.LockAll(It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.Execute("eventId", new ExecuteDto{ Roles = new List<string>{"Roles"}}));

            // Assert
            Assert.Throws<FailedToLockOtherEventException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_FailedToUnlockOtherEventsException_When_Other_Events_Cannot_Be_Locked()
        {
            // Arrange
            _lockingLogicMock.Setup(l => l.UnlockAll(It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.Execute("eventId", new ExecuteDto { Roles = new List<string> { "Roles" } }));

            // Assert
            Assert.Throws<FailedToUnlockOtherEventException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_FailedToUpdateStateException_When_Executed_Cannot_Be_Set()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.SetExecuted(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.Execute("eventId", new ExecuteDto { Roles = new List<string> { "RightRole" } }));

            // Assert
            Assert.Throws<FailedToUpdateStateException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_FailedToUpdateStateException_When_Pending_Cannot_Be_Set()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.SetPending(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateLogic.Execute("eventId", new ExecuteDto { Roles = new List<string> { "RightRole" } }));

            // Assert
            Assert.Throws<FailedToUpdateStateException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_FailedToUpdateStateAtOtherEventException_When_Response_Cannot_Be_Found()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetResponses(It.IsAny<string>()))
                .Returns(new HashSet<RelationToOtherEventModel>
                {
                    new RelationToOtherEventModel
                    {
                        EventID = "NonExistentEventId",
                        Uri = new Uri("http://localhost:65443/")
                    }
                });

            // Act
            var testDelegate =
                new TestDelegate(
                    async () =>
                        await _stateLogic.Execute("eventId", new ExecuteDto {Roles = new List<string> {"RightRole"}}));

            // Assert
            Assert.Throws<FailedToUpdateStateAtOtherEventException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_FailedToUpdateStateAtOtherEventException_When_Inclusion_Cannot_Be_Found()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetInclusions(It.IsAny<string>()))
                .Returns(new HashSet<RelationToOtherEventModel>
                {
                    new RelationToOtherEventModel
                    {
                        EventID = "NonExistentEventId",
                        Uri = new Uri("http://localhost:65443/")
                    }
                });

            // Act
            var testDelegate =
                new TestDelegate(
                    async () =>
                        await _stateLogic.Execute("eventId", new ExecuteDto { Roles = new List<string> { "RightRole" } }));

            // Assert
            Assert.Throws<FailedToUpdateStateAtOtherEventException>(testDelegate);
        }

        [Test]
        public void Execute_Throws_FailedToUpdateStateAtOtherEventException_When_Exclusion_Cannot_Be_Found()
        {
            // Arrange
            _eventStorageMock.Setup(s => s.GetExclusions(It.IsAny<string>()))
                .Returns(new HashSet<RelationToOtherEventModel>
                {
                    new RelationToOtherEventModel
                    {
                        EventID = "NonExistentEventId",
                        Uri = new Uri("http://localhost:65443/")
                    }
                });

            // Act
            var testDelegate =
                new TestDelegate(
                    async () =>
                        await _stateLogic.Execute("eventId", new ExecuteDto { Roles = new List<string> { "RightRole" } }));

            // Assert
            Assert.Throws<FailedToUpdateStateAtOtherEventException>(testDelegate);
        }
    }

}
