using Client.Connections;
using Client.ViewModels;
using Common.DTO.Server;
using Moq;
using NUnit.Framework;

namespace Client.Tests.ViewModels
{
    [TestFixture]
    class LoginViewModelTests
    {
        private LoginViewModel _model;
        private Mock<IServerConnection> _serverConnectionMock;
        private RolesOnWorkflowsDto _rolesOnWorkflowsDto;

        [SetUp]
        public void SetUp()
        {
            _rolesOnWorkflowsDto = new RolesOnWorkflowsDto();

            _serverConnectionMock = new Mock<IServerConnection>(MockBehavior.Strict);
            _serverConnectionMock.Setup(sc => sc.Login(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(_rolesOnWorkflowsDto);

            _model = new LoginViewModel(_serverConnectionMock.Object);
        }

        #region Constructor

        [Test]
        public void Constructor_Ok()
        {
            // Act
            var model = new LoginViewModel();

            // Assert
            Assert.IsNotNull(model);
        }
        #endregion
    }
}
