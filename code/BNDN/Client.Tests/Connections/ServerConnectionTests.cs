﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Client.Connections;
using Client.Exceptions;
using Common.DTO.Event;
using Common.DTO.History;
using Common.DTO.Server;
using Common.DTO.Shared;
using Common.Exceptions;
using Common.Tools;
using Moq;
using NUnit.Framework;

namespace Client.Tests.Connections
{
    [TestFixture]
    public class ServerConnectionTests
    {
        private ServerConnection _connection;
        private Mock<HttpClientToolbox> _toolboxMock;
        private List<WorkflowDto> _workflowDtos;
        private List<EventAddressDto> _eventAddressDtos;
        private List<HistoryDto> _historyDtos;

        [SetUp]
        public void SetUp()
        {
            _workflowDtos = new List<WorkflowDto>();
            _eventAddressDtos = new List<EventAddressDto>();
            _historyDtos = new List<HistoryDto>();

            _toolboxMock = new Mock<HttpClientToolbox>(MockBehavior.Strict);
            _toolboxMock.Setup(t => t.ReadList<WorkflowDto>(It.IsAny<string>())).ReturnsAsync(_workflowDtos);
            _toolboxMock.Setup(t => t.ReadList<EventAddressDto>(It.IsAny<string>())).ReturnsAsync(_eventAddressDtos);
            _toolboxMock.Setup(t => t.ReadList<HistoryDto>(It.IsAny<string>())).ReturnsAsync(_historyDtos);

            _connection = new ServerConnection(_toolboxMock.Object);
        }

        [Test]
        public void Dispose_Test()
        {
            // Arrange
            _toolboxMock.Setup(t => t.Dispose()).Verifiable();

            // Act
            using (_connection)
            {
                // Do nothing.
            }

            // Assert
            _toolboxMock.Verify(t => t.Dispose(), Times.Once);
        }

        [Test]
        public async void GetWorkflows_Ok()
        {
            // Arrange
            _workflowDtos.Add(new WorkflowDto
            {
                Id = "course",
                Name = "Course Workflow"
            });
            _workflowDtos.Add(new WorkflowDto
            {
                Id = "gasstation",
                Name = "Gas station Workflow"
            });

            // Act
            var workflows = await _connection.GetWorkflows();

            // Assert
            Assert.IsNotNull(workflows);
            Assert.IsNotEmpty(workflows);
        }

        [Test]
        public void GetWorkflows_Throws()
        {
            // Arrange
            _toolboxMock.Setup(t => t.ReadList<WorkflowDto>(It.IsAny<string>())).ThrowsAsync(new HttpRequestException());

            // Act
            var testDelegate = new TestDelegate(async () => await _connection.GetWorkflows());

            // Assert
            Assert.Throws<HostNotFoundException>(testDelegate);
        }

        [Test]
        public async void GetWorkflows_Empty()
        {
            // Arrange
            _toolboxMock.Setup(t => t.ReadList<WorkflowDto>(It.IsAny<string>())).ReturnsAsync(new List<WorkflowDto>());

            // Act
            var workflows = await _connection.GetWorkflows();

            // Assert
            Assert.IsNotNull(workflows);
            Assert.IsEmpty(workflows);
        }

        [Test]
        public async void Login_Success()
        {
            // Arrange
            var rolesOnWorkflows = new Dictionary<string, ICollection<string>>
            {
                {
                    "course", new List<string>
                    {
                        "student"
                    }
                }
            };

            var rolesOnWorkflowDto = new RolesOnWorkflowsDto
            {
                RolesOnWorkflows = rolesOnWorkflows
            };

            _toolboxMock.Setup(t => t.Create<LoginDto, RolesOnWorkflowsDto>(It.IsAny<string>(), It.IsAny<LoginDto>()))
                .ReturnsAsync(rolesOnWorkflowDto);

            // Act
            var rolesOnWorkflow = await _connection.Login("testy-testy", "testy-password");

            // Assert
            Assert.IsNotNull(rolesOnWorkflow);
            Assert.AreEqual("student", rolesOnWorkflow.RolesOnWorkflows["course"].First());
        }

        [Test]
        public void Login_Failed()
        {
            // Arrange
            _toolboxMock.Setup(t => t.Create<LoginDto, RolesOnWorkflowsDto>(It.IsAny<string>(), It.IsAny<LoginDto>()))
                .ThrowsAsync(new UnauthorizedException());

            // Act
            var testDelegate = new TestDelegate(async () => await _connection.Login("wrongUsername", "wrongPassword"));

            // Assert
            Assert.Throws<LoginFailedException>(testDelegate);
        }

        [Test]
        public void Login_HostNotFound()
        {
            // Arrange
            _toolboxMock.Setup(t => t.Create<LoginDto, RolesOnWorkflowsDto>(It.IsAny<string>(), It.IsAny<LoginDto>()))
                .ThrowsAsync(new HttpRequestException());

            // Act
            var testDelegate = new TestDelegate(async () => await _connection.Login("wrongUsername", "wrongPassword"));

            // Assert
            Assert.Throws<HostNotFoundException>(testDelegate);
        }

        [Test]
        public async void GetEventsFromWorkflow_Returns_Events()
        {
            // Arrange
            _eventAddressDtos.Add(new EventAddressDto
            {
                Id = "register",
                Uri = new Uri("http://localhost:13752")
            });
            _eventAddressDtos.Add(new EventAddressDto
            {
                Id = "pass",
                Uri = new Uri("http://localhost:13753")
            });
            _eventAddressDtos.Add(new EventAddressDto
            {
                Id = "fail",
                Uri = new Uri("http://localhost:13754")
            });
            
            // Act
            var result = await _connection.GetEventsFromWorkflow("course");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public async void GetEventsFromWorkflow_Returns_Empty()
        {
            // Arrange

            // Act
            var result = await _connection.GetEventsFromWorkflow("course");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetEventsFromWorkflow_Throws_Exception()
        {
            // Arrange
            _toolboxMock.Setup(t => t.ReadList<EventAddressDto>(It.IsAny<string>()))
                .Throws(new HttpRequestException()); //no message, we expect the HostNotFoundException exception

            // Act
            var testDelegate = new TestDelegate(async () => await _connection.GetEventsFromWorkflow("course"));

            // Assert
            Assert.Throws<HostNotFoundException>(testDelegate);
        }

        [Test]
        public async void GetHistory_Ok()
        {
            // Arrange
            _historyDtos.Add(new HistoryDto
            {
                WorkflowId = "workflowId",
                EventId = "eventId"
            });

            // Act
            var history = await _connection.GetHistory("workflowId");

            // Assert
            Assert.IsNotNull(history);
            Assert.IsNotEmpty(history);
        }

        [Test]
        public void GetHistory_Throws()
        {
            // Arrange
            _toolboxMock.Setup(t => t.ReadList<HistoryDto>(It.IsAny<string>())).ThrowsAsync(new HttpRequestException());

            // Act
            var testDelegate = new TestDelegate(async () => await _connection.GetHistory("workflowId"));

            // Assert
            Assert.Throws<HostNotFoundException>(testDelegate);
        }

        [Test]
        public async void GetHistory_Empty()
        {
            // Arrange
            _toolboxMock.Setup(t => t.ReadList<HistoryDto>(It.IsAny<string>())).ReturnsAsync(new List<HistoryDto>());

            // Act
            var workflows = await _connection.GetHistory("workflowId");

            // Assert
            Assert.IsNotNull(workflows);
            Assert.IsEmpty(workflows);
        }
    }
}
