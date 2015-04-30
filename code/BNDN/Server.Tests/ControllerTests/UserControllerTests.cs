using System.Net.Http;
using System.Web.Http;
using Common;
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

        [Test]
        public void LoginWithAUserThatDoesntExistThrowsException()
        {
            //Arrange
            _logicMock.Setup(t => t.Login(It.IsAny<LoginDto>())).ThrowsAsync(new UnauthorizedException());
            
            //Action
            var testDel = new TestDelegate(async () => await _usersController.Login(new LoginDto{Username = "doesntexist", Password = "yay"}));

            //Assert
            Assert.Throws<HttpResponseException>(testDel);
        }

    }
}
