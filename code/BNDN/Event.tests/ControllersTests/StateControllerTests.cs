using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Controllers;
using Event.Exceptions;
using Event.Interfaces;
using Moq;
using NUnit.Framework;

namespace Event.Tests.ControllersTests
{
    [TestFixture]
    class StateControllerTests
    {
        private Mock<IStateLogic> _stateLogicMock;
        private StateController _stateController;

        [SetUp]
        public void SetUp()
        {
            _stateLogicMock = new Mock<IStateLogic>();

            _stateController = new StateController(_stateLogicMock.Object);
        }

        #region GetExecuted
        [Test]
        public async Task GetExecuted_Returns_true()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsExecuted(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _stateController.GetExecuted("eventId", "senderId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetExecuted_Returns_false()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsExecuted(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _stateController.GetExecuted("eventId", "senderId");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetExecuted_NotFound_Throws_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsExecuted(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new NotFoundException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetExecuted("eventId", "senderId"));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void GetExecuted_NotFound_Throws_404_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsExecuted(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new NotFoundException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetExecuted("eventId", "senderId"));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            Assert.AreEqual(HttpStatusCode.NotFound, exception.Response.StatusCode);
        }

        [Test]
        public void GetExecuted_Locked_Throws_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsExecuted(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new LockedException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetExecuted("eventId", "senderId"));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void GetExecuted_Locked_Throws_409_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsExecuted(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new LockedException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetExecuted("eventId", "senderId"));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            Assert.AreEqual(HttpStatusCode.Conflict, exception.Response.StatusCode);
        }
        #endregion

        #region GetIncluded
        [Test]
        public async Task GetIncluded_Returns_true()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsIncluded(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _stateController.GetIncluded("eventId", "senderId");

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetIncluded_Returns_false()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsIncluded(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _stateController.GetIncluded("eventId", "senderId");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetIncluded_NotFound_Throws_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsIncluded(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new NotFoundException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetIncluded("eventId", "senderId"));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void GetIncluded_NotFound_Throws_404_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsIncluded(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new NotFoundException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetIncluded("eventId", "senderId"));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            Assert.AreEqual(HttpStatusCode.NotFound, exception.Response.StatusCode);
        }

        [Test]
        public void GetIncluded_Locked_Throws_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsIncluded(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new LockedException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetIncluded("eventId", "senderId"));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void GetIncluded_Locked_Throws_409_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.IsIncluded(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new LockedException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetIncluded("eventId", "senderId"));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            Assert.AreEqual(HttpStatusCode.Conflict, exception.Response.StatusCode);
        }
        #endregion

        #region GetState

        // Not all cases can occur in our code, but for the sake of testing:
        [TestCase(true, true, true, true)]
        [TestCase(true, true, true, false)]
        [TestCase(true, true, false, true)] // Executable cannot be true when included is false.
        [TestCase(true, true, false, false)] // Executable cannot be true when included is false.
        [TestCase(true, false, true, true)]
        [TestCase(true, false, true, false)]
        [TestCase(true, false, false, true)] // Executable cannot be true when included is false.
        [TestCase(true, false, false, false)] // Executable cannot be true when included is false.
        [TestCase(false, true, true, true)]
        [TestCase(false, true, true, false)]
        [TestCase(false, true, false, true)]
        [TestCase(false, true, false, false)]
        [TestCase(false, false, true, true)]
        [TestCase(false, false, true, false)]
        [TestCase(false, false, false, true)]
        [TestCase(false, false, false, false)]
        public async Task GetState_Returns_Case(bool executable, bool executed, bool included, bool pending)
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.GetStateDto(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(((string eventId, string senderId) => Task.Run(() => new EventStateDto
                {
                    Id = eventId,
                    Name = eventId.ToUpper(),
                    Executable = executable,
                    Executed = executed,
                    Included = included,
                    Pending = pending
                })));

            // Act
            var result = await _stateController.GetState("eventId", "senderId");

            // Assert
            Assert.AreEqual("eventId", result.Id);
            Assert.AreEqual("EVENTID", result.Name);
            Assert.AreEqual(executable, result.Executable);
            Assert.AreEqual(executed, result.Executed);
            Assert.AreEqual(included, result.Included);
            Assert.AreEqual(pending, result.Pending);
        }

        [Test]
        public void GetState_NotFound_Throws_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.GetStateDto(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new NotFoundException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetState("eventId", "senderId"));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void GetState_NotFound_Throws_404_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.GetStateDto(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new NotFoundException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetState("eventId", "senderId"));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            Assert.AreEqual(HttpStatusCode.NotFound, exception.Response.StatusCode);
        }

        [Test]
        public void GetState_Locked_Throws_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.GetStateDto(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new LockedException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetState("eventId", "senderId"));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void GetState_Locked_Throws_409_HttpResponseException()
        {
            // Arrange
            _stateLogicMock.Setup(sl => sl.GetStateDto(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new LockedException());

            _stateController.Request = new HttpRequestMessage();

            // Act
            var testDelegate = new TestDelegate(async () => await _stateController.GetState("eventId", "senderId"));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            Assert.AreEqual(HttpStatusCode.Conflict, exception.Response.StatusCode);
        }
        #endregion

        #region UpdateIncluded
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task UpdateIncluded_Was_x_SetTo_y(bool x, bool y)
        {
            // Arrange
            var logicIncluded = x;

            _stateLogicMock.Setup(sl => sl.SetIncluded(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns((string eventId, string senderId, bool newIncluded) => Task.Run(() => logicIncluded = newIncluded));

            // Act
            // Update included:
            await _stateController.UpdateIncluded("eventId", y, new EventAddressDto {Id = "senderId"});

            // Assert
            Assert.AreEqual(y, logicIncluded);
        }
        #endregion

        #region UpdatePending
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task UpdatePending_Was_x_SetTo_y(bool x, bool y)
        {
            // Arrange
            var logicPending = x;

            _stateLogicMock.Setup(sl => sl.SetPending(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns((string eventId, string senderId, bool newPending) => Task.Run(() => logicPending = newPending));

            // Act
            // Update included:
            await _stateController.UpdatePending("eventId", y, new EventAddressDto { Id = "senderId" });

            // Assert
            Assert.AreEqual(y, logicPending);
        }

        #endregion
    }
}
