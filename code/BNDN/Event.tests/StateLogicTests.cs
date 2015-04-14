using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        [SetUp]
        public void SetUp()
        {
            _eventStorageMock = new Mock<IEventStorage>();
            _lockingLogicMock = new Mock<ILockingLogic>();

            // Make the Event unlocked other has been specified.
            _lockingLogicMock.Setup(l => l.IsAllowedToOperate("eventId", "senderId")).ReturnsAsync(true);

            var authLogic = new Mock<IAuthLogic>();

            _stateLogic = new StateLogic(_eventStorageMock.Object, _lockingLogicMock.Object, authLogic.Object);
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
        public void GetStateDto_Throws_LockedDto()
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
    }

}
