using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Client.Connections;
using Client.Exceptions;
using Common.DTO.Event;
using Common.Exceptions;
using Common.Tools;
using Moq;
using NUnit.Framework;

namespace Client.Tests.Connections
{
    [TestFixture]
    class EventConnectionTests
    {
        private EventConnection _connection;
        private Mock<HttpClientToolbox> _toolboxMock;

        [SetUp]
        public void SetUp()
        {
            _toolboxMock = new Mock<HttpClientToolbox>(MockBehavior.Strict);

            _connection = new EventConnection(_toolboxMock.Object);
        }

        #region Constructor and dispose

        [Test]
        public void EventConnection_No_Arguments()
        {
            // Act
            var conn = new EventConnection();

            // Assert
            Assert.IsNotNull(conn);
        }

        [Test]
        public void Dispose_ok()
        {
            using (var conn = new EventConnection())
            {
                // Do nothing.
            }

            // If no errors happened, all is good.
        }
        #endregion

        #region GetState
        [Test]
        public async Task GetState_Ok()
        {
            // Arrange
            var dto = new EventStateDto();

            _toolboxMock.Setup(t => t.Read<EventStateDto>(It.IsAny<string>()))
                .ReturnsAsync(dto).Verifiable();

            // Act
            var result = await _connection.GetState(new Uri("http://uri.uri"), "workflow", "event");

            // Assert
            Assert.AreSame(dto, result);
            _toolboxMock.Verify(t => t.Read<EventStateDto>(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetState_HostNotFound()
        {
            // Arrange
            _toolboxMock.Setup(t => t.Read<EventStateDto>(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException());

            // Act
            var testDelegate = new TestDelegate(async () => await _connection.GetState(new Uri("http://uri.uri"), "workflow", "event"));

            // Assert
            Assert.Throws<HostNotFoundException>(testDelegate);
            _toolboxMock.Verify(t => t.Read<EventStateDto>(It.IsAny<string>()), Times.Once);
        }

        [TestCase(typeof(NotFoundException)),
         TestCase(typeof(LockedException)),
         TestCase(typeof(NotExecutableException)),
         TestCase(typeof(UnauthorizedException))]
        public void GetState_ExceptionPassthrough(Type exceptionType)
        {
            // Arrange
            var exception = (Exception) exceptionType.GetConstructors().First().Invoke(null);

            _toolboxMock.Setup(t => t.Read<EventStateDto>(It.IsAny<string>()))
                .ThrowsAsync(exception);

            // Act
            var testDelegate = new TestDelegate(async () => await _connection.GetState(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>()));

            // Assert
            var thrown = Assert.Throws(exceptionType, testDelegate);
            Assert.AreSame(exception, thrown);
        }
        #endregion

        #region GetHistory

        [Test]
        public void GetHistory()
        {
            Assert.Fail();
        }
        #endregion
    }
}
