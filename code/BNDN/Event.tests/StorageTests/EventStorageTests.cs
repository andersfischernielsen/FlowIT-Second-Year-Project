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
        private EventIdentificationModel _eim;
        private EventStateModel _esm;

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<IEventContext>();
            _contextMock.Setup(c => c.Dispose());
            _contextMock.SetupAllProperties();

            _eim = new EventIdentificationModel
            {
                Id = "eventId",
                Name = "Event",
                WorkflowId = "workflowId",
                OwnUri = "http://www.contoso.com/",
                Roles = new List<EventRoleModel>
                {
                    new EventRoleModel
                    {
                        EventId = "eventId",
                        Role = "Student"
                    }
                }
            };

            _esm = new EventStateModel
            {
                Id = "eventId",
                EventIdentificationModel = _eim,
                Executed = false,
                Included = true,
                Pending = false
            };

            // Code to get all of the async stuff to work.
            var eventStateModels = new List<EventStateModel>{_esm}.AsQueryable();
            var eventStateMockSet = new FakeDbSet<EventStateModel>(eventStateModels);
            _contextMock.Setup(c => c.EventState).Returns(eventStateMockSet.Object);

            var eventIdentificationModels = new List<EventIdentificationModel>{_eim}.AsQueryable();
            var eventIdentificationMockSet = new FakeDbSet<EventIdentificationModel>(eventIdentificationModels);
            _contextMock.Setup(c => c.EventIdentification).Returns(eventIdentificationMockSet.Object); 

            _eventStorage = new EventStorage(_contextMock.Object);
        }

        #region GetExecuted
        [Test]
        public async Task GetExecuted_Returns_True()
        {
            // Arrange
            _esm.Executed = true;

            // Act
            var result = await _eventStorage.GetExecuted("eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetExecuted_Returns_False()
        {
            // Act
            var result = await _eventStorage.GetExecuted("eventId");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetExecuted_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetExecuted("notEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }
        #endregion
        #region GetPending
        [Test]
        public async Task GetPending_Returns_True()
        {
            // Arrange
            _esm.Pending = true;

            // Act
            var result = await _eventStorage.GetPending("eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetPending_Returns_False()
        {
            // Arrange

            // Act
            var result = await _eventStorage.GetPending("eventId");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetPending_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetPending("notEventId"));

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
            var result = await _eventStorage.GetIncluded("eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetIncluded_Returns_False()
        {
            // Arrange
            _esm.Included = false;

            // Act
            var result = await _eventStorage.GetIncluded("eventId");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetIncluded_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetIncluded("notEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }
        #endregion
        #region Exists

        [Test]
        public async Task Exists_Returns_True()
        {
            // Act
            var result = await _eventStorage.Exists("eventId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task Exists_Returns_False()
        {
            // Act
            var result = await _eventStorage.Exists("notEventId");

            // Assert
            Assert.IsFalse(result);
        }
        #endregion
        #region GetName

        [Test]
        public async Task GetName_Returns_Event()
        {
            // Act
            var result = await _eventStorage.GetName("eventId");

            // Assert
            Assert.AreEqual("Event", result);
        }

        [Test]
        public void GetName_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetName("wrongEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }
        #endregion
        #region GetRoles

        [Test]
        public async Task GetRoles_Returns_List()
        {
            // Act
            var result = (await _eventStorage.GetRoles("eventId")).ToList();

            // Assert
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Contains("Student"));
        }

        [Test]
        public void GetRoles_Throws_NotFoundException()
        {
            // Act
            var testDelegate = new TestDelegate(async () => await _eventStorage.GetRoles("wrongEventId"));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }
        #endregion
    }
}
