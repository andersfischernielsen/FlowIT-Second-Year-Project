using System;
using Event.Communicators;
using Event.Exceptions;
using Event.Exceptions.EventInteraction;
using NUnit.Framework;

namespace Event.Tests.CommunicatorTests
{
    [TestFixture]
    public class EventCommunicatorTests
    {
        private ServerCommunicator _toTest;
        private Mock<IHttpClientToolbox> _toolBoxMock;
        
        [TestFixtureSetup]
        public void Setup() {
            var mock = new Mock<HttpClientToolbox>();
            
            mock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<EventAddressDto>())).Verifiable();
            mock.Setup(m => m.Delete(It.IsAny<string>())).Verifiable();
            mock.Setup(m => m.Dispose()).Verifiable();
            
        }
        
        [Test]
        public void ConstructorTest() {
            Assert.Throws<ArgumentNullException>(_toTest = new ServerCommunicator(null, "", ""))
            Assert.Throws<ArgumentNullException>(_toTest = new ServerCommunicator("", null, ""))
            Assert.Throws<ArgumentNullException>(_toTest = new ServerCommunicator("", "", null))
        }

        [Test]
        public void PostEventToServerTest() {
            
        }
        
        [Test]
        public void DeleteEventFromServerTest() {
            
        }
        
        [Test]
        public void DisposeTest() {
            
        }
    }
}
