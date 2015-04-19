using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event.Exceptions;
using Event.Interfaces;
using Event.Logic;
using Moq;
using NUnit.Framework;

namespace Event.Tests.LogicTests
{
    [TestFixture]
    class AuthLogicTests
    {
        private Mock<IEventStorage> _storageMock;
        private AuthLogic _logic;

        [SetUp]
        public void SetUp()
        {
            _storageMock = new Mock<IEventStorage>();
            _storageMock.Setup(s => s.Dispose());

            _logic = new AuthLogic(_storageMock.Object);
        }

        [Test]
        public async Task IsAuthorized_Returns_True()
        {
            // Arrange
            _storageMock.Setup(s => s.GetRoles(It.IsAny<string>())).ReturnsAsync(new HashSet<string>
            {
                "Student",
                "Teacher"
            });
            
            // Act
            var result = await _logic.IsAuthorized("eventId", new List<string> { "Student" });

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task IsAuthorized_Returns_False()
        {
            // Arrange
            _storageMock.Setup(s => s.GetRoles(It.IsAny<string>())).ReturnsAsync(new HashSet<string>
            {
                "Student",
                "Teacher"
            });

            // Act
            var result = await _logic.IsAuthorized("eventId", new List<string> { "Miner" });

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsAuthorized_Throws_ArgumentNullException_When_Passed_Roles_Is_NULL()
        {
            // Arrange
            _storageMock.Setup(s => s.GetRoles(It.IsAny<string>())).ReturnsAsync(new HashSet<string>
            {
                "Student",
                "Teacher"
            });

            // Act
            var testDelegate = new TestDelegate(async () => await _logic.IsAuthorized("eventId", null));

            // Assert
            Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Test]
        public void IsAuthorized_Throws_NotFoundException_When_EventId_Is_Not_Found()
        {
            // Arrange
            _storageMock.Setup(s => s.GetRoles(It.IsAny<string>())).ReturnsAsync(null);

            // Act
            var testDelegate = new TestDelegate(async () => await _logic.IsAuthorized("eventId", new HashSet<string>
            {
                "Student",
                "Teacher"
            }));

            // Assert
            Assert.Throws<NotFoundException>(testDelegate);
        }
    }
}
