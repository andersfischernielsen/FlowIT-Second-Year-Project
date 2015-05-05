using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event.Interfaces;
using Event.Models;
using Event.Storage;
using Moq;
using NUnit.Framework;

namespace Event.Tests.StorageTests
{
    [TestFixture]
    class EventStorageForResetTests
    {
        private EventStorageForReset _storageForReset;
        private Mock<IEventContext> _contextMock;

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<IEventContext>(MockBehavior.Strict);
            _contextMock.Setup(c => c.Dispose()).Verifiable();

            _storageForReset = new EventStorageForReset(_contextMock.Object);
        }

        #region Constructor and Dispose
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Null()
        {
            // Act
            var storageForReset = new EventStorageForReset(null);

            // Assert
            Assert.Fail("Constructor did not fail: {0}", storageForReset.GetType()); // Should not be run.
        }

        [Test]
        public void Constructor_ValidArgument()
        {
            // Act
            var storageForReset = new EventStorageForReset(_contextMock.Object);

            // Assert
            Assert.IsNotNull(storageForReset);
        }

        [Test]
        public void Dispose_Ok()
        {
            // Act
            using (_storageForReset)
            {

            }

            // Assert
            _contextMock.Verify(c => c.Dispose(), Times.Once);
        }
        #endregion

        #region Exists
        [TestCase(null, "eventId"),
         TestCase(null, null),
         TestCase("workflowId", null)]
        public void Exists_ArgumentNull(string workflowId, string eventId)
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _storageForReset.Exists(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public async Task Exists_True()
        {
            // Arrange
            var events = new List<EventModel>{ new EventModel
            {
                WorkflowId = "workflowId",
                Id = "eventId"
            }}.AsQueryable();

            _contextMock.Setup(c => c.Events).Returns(new FakeDbSet<EventModel>(events).Object);

            // Act
            var result = await _storageForReset.Exists("workflowId", "eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task Exists_False()
        {
            // Arrange
            var events = new List<EventModel>().AsQueryable();

            _contextMock.Setup(c => c.Events).Returns(new FakeDbSet<EventModel>(events).Object);

            // Act
            var result = await _storageForReset.Exists("workflowId", "eventId");

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region ResetToInitialState

        [TestCase(true, true, true),
         TestCase(true, true, false),
         TestCase(true, false, true),
         TestCase(true, false, false),
         TestCase(false, true, true),
         TestCase(false, true, false),
         TestCase(false, false, true),
         TestCase(false, false, false)]
        public async Task ResetToInitialState_Ok(bool initialExecuted, bool initialIncluded, bool initialPending)
        {
            // Arrange
            // Make Exists return true and set initial states
            var events = new List<EventModel>{ new EventModel
            {
                WorkflowId = "workflowId",
                Id = "eventId",
                Executed = !initialExecuted,
                Included = !initialIncluded,
                Pending = !initialPending,
                InitialExecuted = initialExecuted,
                InitialIncluded = initialIncluded,
                InitialPending = initialPending
            }}.AsQueryable();

            _contextMock.Setup(c => c.Events).Returns(new FakeDbSet<EventModel>(events).Object);
            _contextMock.Setup(c => c.SaveChangesAsync()).Returns(Task.FromResult(1));

            // Act
            await _storageForReset.ResetToInitialState("workflowId", "eventId");

            // Assert
            Assert.AreEqual(initialExecuted, events.First().Executed);
            Assert.AreEqual(initialIncluded, events.First().Included);
            Assert.AreEqual(initialPending, events.First().Pending);
        }
        #endregion
    }
}
