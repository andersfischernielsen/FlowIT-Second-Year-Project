﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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

        private IEnumerable<WorkflowRole> GetSomeRoles()
        {
            var roles = new List<WorkflowRole>
            {
                new WorkflowRole() {Role = "Ambassador", Workflow = "Healthcare"},
                new WorkflowRole() {Role = "Governor", Workflow = "Healthcare"},
                new WorkflowRole() {Role = "President", Workflow = "Healtchcare"},
                new WorkflowRole() {Role = "Nurse", Workflow = "Healthcare"}
            };

            return roles;
        }

        private UserDto GetValidUserDto()
        {
            UserDto returnDto = new UserDto()
            {
                Name = "Hans",
                Password = "abcdef123",
                Roles = GetSomeRoles()
            };

            return returnDto;
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

        [Test]
        public void CreateUser_WillHandleArgumentExceptionCorrectly_2()
        {
            // Arrange
            var exceptionToBeThrown = new ArgumentException();
            _logicMock.Setup(m => m.AddUser(It.IsAny<UserDto>())).Throws(exceptionToBeThrown);
            UserDto provideDto = GetValidUserDto();

            // Act
            var task = _usersController.CreateUser(provideDto);

            // Assert
            var exception = task.Exception.InnerException as HttpResponseException;
            if (exception == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }


        [Test]
        public void CreateUser_ThrowsExceptionWhenProvidedNonMappableInput()
        {
            // Arrange
            _usersController.ModelState.AddModelError("Name",new ArgumentNullException());
            var userDto = new UserDto();

            // Act
            var task = _usersController.CreateUser(userDto);

            var exception = task.Exception.InnerException as HttpResponseException;
            if (exception == null)
            {
                Assert.Fail();
            }

            // Assert
            Assert.IsInstanceOf<HttpResponseException>(exception);
            Assert.AreEqual(HttpStatusCode.BadRequest,exception.Response.StatusCode);
        }
        #endregion

        #region AddRolesToUser

        [Test]
        public void AddRolesToUser_ThrowsExceptionWhenProvidedNonMappableInput()
        {
            // Arrange
            _usersController.ModelState.AddModelError("Role",new ArgumentNullException());
            var rolesList = GetSomeRoles();
            // Act
            var testDelegate = new TestDelegate(async() => await _usersController.AddRolesToUser("Hanne", GetSomeRoles()));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }

        [Test]
        public void AddRolesToUser_WillLogWhenProvidedNonMappableInput()
        {
            // Arrange
            bool logWasCalled = false;
            _usersController.ModelState.AddModelError("Name",new ArgumentNullException());
            _historyLogic.Setup(m => m.SaveNoneWorkflowSpecificHistory(It.IsAny<HistoryModel>()))
                .Callback((HistoryModel model) => logWasCalled = true);
            var rolesList = GetSomeRoles();


            // Act
            _usersController.AddRolesToUser("Hanne", rolesList);

            // Assert
            Assert.IsTrue(logWasCalled);
        }


        [Test]
        public void AddRolesToUser_HandsOverInputUnaffectedToLogicLayer()
        {
            // Arrange
            string user = "Hanne";
            string receivedUserOnLogicSide = null;
            IEnumerable<WorkflowRole> rolesList = GetSomeRoles();
            IEnumerable<WorkflowRole> rolesListReceivedOnLogicSide = null;
            _logicMock.Setup(m => m.AddRolesToUser(It.IsAny<string>(), It.IsAny<IEnumerable<WorkflowRole>>())).
                Callback((string u, IEnumerable<WorkflowRole> roles) =>
                {
                    receivedUserOnLogicSide = u;
                    rolesListReceivedOnLogicSide = roles;
                });

            // Act
            _usersController.AddRolesToUser(user, rolesList);

            // Assert
            Assert.AreEqual(user,receivedUserOnLogicSide);
            Assert.AreEqual(rolesList,rolesListReceivedOnLogicSide);
        }

        [TestCase(typeof(ArgumentNullException))]
        [TestCase(typeof(NotFoundException))]
        [TestCase(typeof(Exception))]
        public void AddRolesToUser_HandlesExceptionCorrectly_1(Type exceptionType)
        {
            // Arrange
            _logicMock.Setup(m => m.AddRolesToUser(It.IsAny<string>(), It.IsAny<IEnumerable<WorkflowRole>>()))
                .Throws((Exception)exceptionType.GetConstructors().First().Invoke(null));
            var rolesList = GetSomeRoles();
            var user = "Hanne";

            // Act
            var testDelegate = new TestDelegate(async () => await _usersController.AddRolesToUser(user, rolesList));

            // Assert
            Assert.Throws<HttpResponseException>(testDelegate);
        }


        [TestCase(typeof (ArgumentNullException), HttpStatusCode.BadRequest)]
        [TestCase(typeof (NotFoundException), HttpStatusCode.NotFound)]
        [TestCase(typeof (Exception), HttpStatusCode.BadRequest)]
        public void AddRolesToUser_HandlesExceptionCorrectly_2(Type exceptionType, HttpStatusCode statuscode)
        {
            // Arrange
            _logicMock.Setup(m => m.AddRolesToUser(It.IsAny<string>(), It.IsAny<IEnumerable<WorkflowRole>>()))
                .Throws((Exception)exceptionType.GetConstructors().First().Invoke(null));
            var rolesList = GetSomeRoles();
            var user = "Hanne";

            // Act
            var task = _usersController.AddRolesToUser(user, rolesList);

            // Assert
            var exception = task.Exception.InnerException as HttpResponseException;
            if (exception == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(statuscode,exception.Response.StatusCode);
        }
        #endregion
    }
}
