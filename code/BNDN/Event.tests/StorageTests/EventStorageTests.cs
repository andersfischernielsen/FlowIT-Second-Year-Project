using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.DTO.History;
using Common.Exceptions;
using Event.Interfaces;
using Event.Models;
using Event.Storage;
using Moq;
using NUnit.Framework;

namespace Event.Tests.StorageTests
{
    [TestFixture]
    class EventStorageTests
    {
        private Mock<IEventContext> _contextMock;
        private EventStorage _eventStorage;
        private List<EventModel> _eventModels;
        private List<HistoryModel> _historyModels;

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<IEventContext>();
            _contextMock.Setup(c => c.Dispose()).Verifiable();
            _contextMock.SetupAllProperties();

            _eventModels = new List<EventModel>
            { 
                new EventModel 
                {
                    Id = "eventId",
                    Name = "Event",
                    WorkflowId = "workflowId",
                    OwnUri = "http://www.contoso.com/",
                    Roles = new List<EventRoleModel>
                    {
                        new EventRoleModel
                        {
                            WorkflowId = "workflowId",
                            EventId = "eventId",
                            Role = "Student"
                        }
                    },
                    Executed = false,
                    Included = true,
                    Pending = false
                }
            };
            _contextMock.Setup(c => c.Events).Returns(new FakeDbSet<EventModel>(_eventModels.AsQueryable()).Object);

            _historyModels = new List<HistoryModel>();
            _contextMock.Setup(c => c.History).Returns(new FakeDbSet<HistoryModel>(_historyModels.AsQueryable()).Object);

            _eventStorage = new EventStorage(_contextMock.Object);
        }

        #region Constructor and Dispose
        [Test]
        public void Constructor_NoArguments()
        {
            // Act
            var storage = new EventStorage();

            // Assert
            Assert.IsNotNull(storage);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullArgument()
        {
            // Act
            var storage = new EventStorage(null);

            // Assert
            Assert.Fail("This should not happen: {0}", storage.GetType());
        }

        [Test]
        public void Constructor_Ok()
        {
            // Act
            var storage = new EventStorage(_contextMock.Object);

            // Assert
            Assert.IsNotNull(storage);
        }

        [Test]
        public void Dispose_Ok()
        {
            // Act
            using (_eventStorage)
            {
                
            }

            // Assert
            _contextMock.Verify(c => c.Dispose(), Times.Once);
        }
        #endregion

        #region GetExecuted
        [Test]
        public async Task GetExecuted_Returns_True()
        {
            // Arrange
            _eventModels.First().Executed = true;

            // Act
            var result = await _eventStorage.GetExecuted("workflowId", "eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetExecuted_Returns_False()
        {
            // Act
            var result = await _eventStorage.GetExecuted("workflowId", "eventId");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetExecuted_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetExecuted("notWorkflowId", "notEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [TestCase(null, null),
         TestCase(null, "eventId"),
         TestCase("workflowId", null)]
        public void GetExecuted_ArgumentNull(string workflowId, string eventId)
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetExecuted(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }
        #endregion

        #region GetPending
        [Test]
        public async Task GetPending_Returns_True()
        {
            // Arrange
            _eventModels.First().Pending = true;

            // Act
            var result = await _eventStorage.GetPending("workflowId", "eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetPending_Returns_False()
        {
            // Arrange

            // Act
            var result = await _eventStorage.GetPending("workflowId", "eventId");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetPending_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetPending("notWorkflowId", "notEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [TestCase(null, null),
         TestCase(null, "eventId"),
         TestCase("workflowId", null)]
        public void GetPending_ArgumentNull(string workflowId, string eventId)
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetPending(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }
        #endregion

        #region GetIncluded
        [Test]
        public async Task GetIncluded_Returns_True()
        {
            // Arrange

            // Act
            var result = await _eventStorage.GetIncluded("workflowId", "eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetIncluded_Returns_False()
        {
            // Arrange
            _eventModels.First().Included = false;

            // Act
            var result = await _eventStorage.GetIncluded("workflowId", "eventId");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetIncluded_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetIncluded("notWorkflowId", "notEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [TestCase(null, null),
         TestCase(null, "eventId"),
         TestCase("workflowId", null)]
        public void GetIncluded_ArgumentNull(string workflowId, string eventId)
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetIncluded(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }
        #endregion

        #region Exists

        [Test]
        public async Task Exists_Returns_True()
        {
            // Act
            var result = await _eventStorage.Exists("workflowId", "eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task Exists_Returns_False()
        {
            // Act
            var result = await _eventStorage.Exists("notWorkflowId", "notEventId");

            // Assert
            Assert.IsFalse(result);
        }

        [TestCase(null, null),
         TestCase(null, "eventId"),
         TestCase("workflowId", null)]
        public void Exists_ArgumentNull(string workflowId, string eventId)
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.Exists(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }
        #endregion

        #region GetName

        [Test]
        public async Task GetName_Returns_Event()
        {
            // Act
            var result = await _eventStorage.GetName("workflowId", "eventId");

            // Assert
            Assert.AreEqual("Event", result);
        }

        [Test]
        public void GetName_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetName("wrongWorkflowId", "wrongEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [TestCase(null, null),
         TestCase(null, "eventId"),
         TestCase("workflowId", null)]
        public void GetName_ArgumentNull(string workflowId, string eventId)
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetName(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }
        #endregion

        #region GetRoles

        [Test]
        public async Task GetRoles_Returns_List()
        {
            // Act
            var result = (await _eventStorage.GetRoles("workflowId", "eventId")).ToList();

            // Assert
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Contains("Student"));
        }

        [Test]
        public void GetRoles_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetRoles("wrongWorkflowId", "wrongEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [TestCase(null, null),
         TestCase(null, "eventId"),
         TestCase("workflowId", null)]
        public void GetRoles_ArgumentNull(string workflowId, string eventId)
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetRoles(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }
        #endregion

        #region GetHistoryForEvent
        [Test]
        public async Task GetHistoryForEvent_Returns_Histories()
        {
            // Arrange
            _historyModels.Add(new HistoryModel
            {
                WorkflowId = "workflowId",
                EventId = "eventId"
            });

            // Act
            var result = await _eventStorage.GetHistoryForEvent("workflowId", "eventId");

            // Assert
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void GetHistoryForEvent_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetHistoryForEvent("wrongWorkflowId", "wrongEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }

        [TestCase(null, null),
         TestCase(null, "eventId"),
         TestCase("workflowId", null)]
        public void GetHistoryForEvent_ArgumentNull(string workflowId, string eventId)
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetHistoryForEvent(workflowId, eventId));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }
        #endregion
    }
}
