using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event.Exceptions;
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
        private EventModel _em;

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<IEventContext>();
            _contextMock.Setup(c => c.Dispose());
            _contextMock.SetupAllProperties();

            _em = new EventModel
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
            };

            // Code to get all of the async stuff to work.
            var eventModels = new List<EventModel>{_em}.AsQueryable();
            var eventMockSet = new FakeDbSet<EventModel>(eventModels);
            _contextMock.Setup(c => c.Events).Returns(eventMockSet.Object); 

            _eventStorage = new EventStorage(_contextMock.Object);
        }

        #region GetExecuted
        [Test]
        public async Task GetExecuted_Returns_True()
        {
            // Arrange
            _em.Executed = true;

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
        #endregion
        #region GetPending
        [Test]
        public async Task GetPending_Returns_True()
        {
            // Arrange
            _em.Pending = true;

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
            _em.Included = false;

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
        #endregion
    }
}
