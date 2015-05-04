using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.DTO.History;
using Common.DTO.Server;
using Common.Exceptions;
using Moq;
using NUnit.Framework;
using Server.Controllers;
using Server.Interfaces;
using Server.Logic;

namespace Server.Tests.ControllerTests
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IServerLogic> _logicMock;
        private UsersController _usersController;
        private Mock<IWorkflowHistoryLogic> _historyLogic;

        [SetUp]
        public void SetUp()
        {
            _logicMock = new Mock<IServerLogic>();
            _historyLogic = new Mock<IWorkflowHistoryLogic>();
            
            _usersController = new UsersController(_logicMock.Object, _historyLogic.Object) {Request = new HttpRequestMessage()};
        }

        #region Login
        [Test]
        public void LoginReturnsRolesOnExistingUser()
        {
            // Arrange
            var loginDto = new LoginDto() {Username = "Hans", Password = "1234"};
            var rolesDictionary = new Dictionary<string, IList<string>>();
            var roles = new List<string> {"Inspector", "Administrator", "Receptionist"};
            rolesDictionary.Add("Hans",roles);

            var returnDto = new RolesOnWorkflowsDto();
            returnDto.RolesOnWorkflows = rolesDictionary;

            _logicMock.Setup(m => m.Login(It.IsAny<LoginDto>())).Returns(Task.Run(() => returnDto));

            // Act
            var result = _usersController.Login(loginDto).Result;

            // Assert
            Assert.AreEqual(returnDto,result);
        }

        [Test]
        public void Login_LogsWhenRolesAreSuccesfullyReturned()
        {
            // Arrange
            bool logMethodWasCalled = false;
            var loginDto = new LoginDto() { Username = "Hans", Password = "1234" };
            var rolesDictionary = new Dictionary<string, IList<string>>();

            var returnDto = new RolesOnWorkflowsDto();
            returnDto.RolesOnWorkflows = rolesDictionary;

            _logicMock.Setup(m => m.Login(It.IsAny<LoginDto>())).Returns(Task.Run(() => returnDto));

            _historyLogic.Setup(m => m.SaveNoneWorkflowSpecificHistory(It.IsAny<HistoryModel>()))
                .Callback((HistoryModel history) => logMethodWasCalled = true);

            // Act
            _usersController.Login(loginDto);

            // Assert
            Assert.IsTrue(logMethodWasCalled);
        }

        [Test]
        public void Login_HandsOverLoginDtoUnAffectedToLogicLayer()
        {
            // Arrange
            var inputlist = new List<LoginDto>();
            _logicMock.Setup(m => m.Login(It.IsAny<LoginDto>())).Callback((LoginDto dto) => inputlist.Add(dto));
            var inputDto = new LoginDto() {Username = "Hans", Password = "snah123"};

            // Act
            _usersController.Login(inputDto);

            // Assert
            Assert.AreEqual(inputDto,inputlist.First());
        }

        [TestCase(typeof(ArgumentNullException))]
        [TestCase(typeof(UnauthorizedException))]
        [TestCase(typeof(Exception))]
        public void Login_WillThrowHttpResponseExceptionWhenCatchingExceptionFromLogic3(Type exceptionType)
        {
            // Arrange
            var loginDto = new LoginDto();
            _logicMock.Setup(m => m.Login(It.IsAny<LoginDto>())).ThrowsAsync((Exception) exceptionType.GetConstructors().First().Invoke(null));

            // Act
            var testDelegate = new TestDelegate(async () => await _usersController.Login(loginDto));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [TestCase(typeof(UnauthorizedException))]
        [TestCase(typeof(Exception))]
        public void Login_WhenExceptionIsThrownHistoryIsCalled1(Type exceptionType)
        {
            // Arrange
            bool logMethodWasCalled = false;
            var loginDto = new LoginDto() { Username = "Hans", Password = "1234" };

            _logicMock.Setup(m => m.Login(It.IsAny<LoginDto>())).ThrowsAsync((Exception)exceptionType.GetConstructors().First().Invoke(null));

            _historyLogic.Setup(m => m.SaveNoneWorkflowSpecificHistory(It.IsAny<HistoryModel>()))
                .Callback((HistoryModel history) => logMethodWasCalled = true);

            // Act
            _usersController.Login(loginDto);

            // Assert
            Assert.IsTrue(logMethodWasCalled);
        }

        [Test]
        public void Login_WhenExceptionIsThrownHistoryIsCalled2()
        {
            // Arrange
            bool logMethodWasCalled = false;
            var loginDto = new LoginDto() { Username = "Hans", Password = "1234" };

            _logicMock.Setup(m => m.Login(It.IsAny<LoginDto>())).ThrowsAsync(new ArgumentNullException());

            _historyLogic.Setup(m => m.SaveHistory(It.IsAny<HistoryModel>()))
                .Callback((HistoryModel history) => logMethodWasCalled = true);

            // Act
            _usersController.Login(loginDto);

            // Assert
            Assert.IsTrue(logMethodWasCalled);
        }

        [Test]
        public void Login_ThrowsExceptionWhenProvidedNullArgument()
        {
            // Arrange
            LoginDto loginDto = null;

            // Act
            var testDelegate = new TestDelegate(async () => await _usersController.Login(loginDto));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void Login_CallsHistoryWhenProvidedNullArgument()
        {
            // Arrange
            bool hasLogged = false;
            LoginDto loginDto = null;
            _historyLogic.Setup(m => m.SaveNoneWorkflowSpecificHistory(It.IsAny<HistoryModel>()))
                .Callback((HistoryModel model) => hasLogged = true);

            // Act
            var testDelegate = new TestDelegate(async () => await _usersController.Login(loginDto));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        #endregion

        #region CreateUser

        #endregion
    }
}
