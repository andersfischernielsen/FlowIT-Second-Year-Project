using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.DTO.Event;
using Common.DTO.History;
using Common.DTO.Server;
using Common.Exceptions;
using Moq;
using NUnit.Framework;
using Server.Controllers;
using Server.Exceptions;
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
            var rolesDictionary = new Dictionary<string, ICollection<string>>();
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
            var rolesDictionary = new Dictionary<string, ICollection<string>>();

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

        [Test]
        public void CreateUser_RaisesExceptionWhenCalledWithNullArgument()
        {
            // Arrange
            UserDto nullUserDto = null;

            // Act
            var testDelegate = new TestDelegate(async () => await _usersController.CreateUser(nullUserDto));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void CreateUser_WillLogIfCalledWithNullArgument()
        {
            // Arrange
            bool logWasCalled = false;
            UserDto nullUserDto = null;
            _historyLogic.Setup(m => m.SaveNoneWorkflowSpecificHistory(It.IsAny<HistoryModel>()))
                .Callback((HistoryModel model) => logWasCalled = true);

            // Act
            _usersController.CreateUser(nullUserDto);

            // Assert
            Assert.IsTrue(logWasCalled);
        }

        [Test]
        public void CreateUser_WillForwardUserDtoUnAffectedToLogicLayer()
        {
            // Arrange
            var catchArgumentList = new List<UserDto>();
            _logicMock.Setup(m => m.AddUser(It.IsAny<UserDto>()))
                .Callback((UserDto providedDto) => catchArgumentList.Add(providedDto));
            var rolesList = new List<WorkflowRole>();
            rolesList.Add(new WorkflowRole(){Role = "Ambassador",Workflow = "Healthcare"});
            UserDto argumentToProvide = new UserDto()
            {
                Name = "Otto",
                Password = "MargaretThatcher",
                Roles = rolesList
            };

            // Act
            var testDelegate = _usersController.CreateUser(argumentToProvide);

            // Assert
            var actualElementThatWasPassedOn = catchArgumentList.First();
            Assert.AreEqual(argumentToProvide,actualElementThatWasPassedOn);
        }

        [TestCase(typeof(ArgumentNullException))]
        [TestCase(typeof(NotFoundException))]
        [TestCase(typeof(UserExistsException))]
        [TestCase(typeof(InvalidOperationException))]
        [TestCase(typeof(ArgumentException))]
        [TestCase(typeof(Exception))]
        public void CreateUser_WillCatchAndConvertException(Type exceptionType)
        {
            // Arrange
            _logicMock.Setup(m => m.AddUser(It.IsAny<UserDto>())).Throws((Exception)exceptionType.GetConstructors().First().Invoke(null));
            var rolesList = new List<WorkflowRole>();
            rolesList.Add(new WorkflowRole(){Role = "Ambassador",Workflow = "Healthcare"});
            UserDto argumentToProvide = new UserDto()
            {
                Name = "Otto",
                Password = "MargaretThatcher",
                Roles = rolesList
            };

            // Act
            var testDelegate = new TestDelegate(async () => await _usersController.CreateUser(argumentToProvide));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [TestCase(typeof(ArgumentNullException))]
        [TestCase(typeof(NotFoundException))]
        [TestCase(typeof(UserExistsException))]
        public void CreateUser_WillLogWhenAnExceptionWasThrown_1(Type exceptionType)
        {
            // Arrange
            bool logWasCalled = false;
            _logicMock.Setup(m => m.AddUser(It.IsAny<UserDto>())).Throws((Exception)exceptionType.GetConstructors().First().Invoke(null));
            _historyLogic.Setup(m => m.SaveHistory(It.IsAny<HistoryModel>())).Callback((HistoryModel x) => logWasCalled = true);

            UserDto argumentToProvide = new UserDto();

            // Act
            _usersController.CreateUser(argumentToProvide);

            // Assert
            Assert.IsTrue(logWasCalled);
        }


        [TestCase(typeof(InvalidOperationException))]
        [TestCase(typeof(ArgumentException))]
        [TestCase(typeof(Exception))]
        public void CreateUser_WillLogWhenAnExceptionWasThrown_2(Type exceptionType)
        {
            // Arrange
            bool logWasCalled = false;
            _logicMock.Setup(m => m.AddUser(It.IsAny<UserDto>())).Throws((Exception)exceptionType.GetConstructors().First().Invoke(null));
            _historyLogic.Setup(m => m.SaveNoneWorkflowSpecificHistory(It.IsAny<HistoryModel>())).Callback((HistoryModel x) => logWasCalled = true);

            UserDto argumentToProvide = new UserDto();

            // Act
            _usersController.CreateUser(argumentToProvide);

            // Assert
            Assert.IsTrue(logWasCalled);
        }

        [Test]
        public void CreateUser_WillHandleArgumentExceptionCorrectly_1()
        {
            // Arrange
            var exceptionToBeThrown = new ArgumentException("Conflicting name", "user");
            _logicMock.Setup(m => m.AddUser(It.IsAny<UserDto>())).Throws(exceptionToBeThrown);
            UserDto provideDto = new UserDto();

            // Act
            var task = _usersController.CreateUser(provideDto);

            // Assert
            var exception = task.Exception.InnerException as HttpResponseException;
            if (exception == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(HttpStatusCode.Conflict,exception.Response.StatusCode);
        }

        /*[Test]
        public void CreateUser_WillHandleArgumentExceptionCorrectly_2()
        {
            // Arrange
            var exceptionToBeThrown = new ArgumentException();
            _logicMock.Setup(m => m.AddUser(It.IsAny<UserDto>())).Throws(exceptionToBeThrown);
            UserDto provideDto = new UserDto();

            // Act
            var task = _usersController.CreateUser(provideDto);

            // Assert
            var exception = task.Exception.InnerException as HttpResponseException;
            if (exception == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }*/

        #endregion
    }
}
