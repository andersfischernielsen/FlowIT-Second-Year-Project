using System;
using System.Collections.Generic;
using System.Net.Http;
using Client.Connections;
using Common;
using Moq;
using NUnit.Framework;

namespace Client.Tests
{
    [TestFixture]
    public class ServerConnectionTests
    {

        [Test]
        public async void GetWorkflows_Ok()
        {
            // Arrange
            var m = new Mock<HttpClientToolbox>(new Uri("http://someUri/"), null);
            var workflowlist = new List<WorkflowDto>
            {
                new WorkflowDto
                {
                    Id = "course",
                    Name = "Course Workflow"
                },
                new WorkflowDto
                {
                    Id = "gasstation",
                    Name = "Gas station Workflow"
                }
            };
            m.Setup(t => t.ReadList<WorkflowDto>(It.IsAny<string>())).ReturnsAsync(workflowlist);

            var serverConnection = new ServerConnection(m.Object);

            // Act
            var workflows = await serverConnection.GetWorkflows();

            // Assert
            Assert.IsNotNull(workflows);
            Assert.IsNotEmpty(workflows);
        }

        [Test]
        public void GetWorkflows_Throws()
        {
            // Arrange
            var m = new Mock<HttpClientToolbox>(new Uri("http://someUri/"), null);
            m.Setup(t => t.ReadList<WorkflowDto>(It.IsAny<string>())).ThrowsAsync(new HttpRequestException());

            var serverConnection = new ServerConnection(m.Object);

            // Act
            var testDelegate = new TestDelegate(async () => await serverConnection.GetWorkflows());

            // Assert
            Assert.Throws<HttpRequestException>(testDelegate);
            Assert.Fail("Should the exception just propagate further up?" +
                        "Or should it be turned into another exception which is easier to respond to?");
        }

        [Test]
        public async void GetWorkflows_Empty()
        {
            // Arrange
            var m = new Mock<HttpClientToolbox>(new Uri("http://someUri/"), null);
            m.Setup(t => t.ReadList<WorkflowDto>(It.IsAny<string>())).ReturnsAsync(new List<WorkflowDto>());

            var serverConnection = new ServerConnection(m.Object);

            // Act
            var workflows = await serverConnection.GetWorkflows();

            // Assert
            Assert.IsNotNull(workflows);
            Assert.IsEmpty(workflows);
        }

        [Test]
        public async void Login_Success()
        {
            // Arrange
            var m = new Mock<HttpClientToolbox>(new Uri("http://someUri/"), null);

            var rolesOnWorkflows = new Dictionary<string, IList<string>>
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

            m.Setup(t => t.Read<RolesOnWorkflowsDto>(It.IsAny<string>()))
                .ReturnsAsync(rolesOnWorkflowDto);

            var serverConnection = new ServerConnection(m.Object);

            // Act
            var rolesOnWorkflow = await serverConnection.Login("testy-testy");

            // Assert
            Assert.IsNotNull(rolesOnWorkflow);
            Assert.AreEqual("student", rolesOnWorkflow.RolesOnWorkflows["course"][0]);
        }

        [Test]
        public void Login_Error()
        {
            // Arrange
            var m = new Mock<HttpClientToolbox>(new Uri("http://someUri/"), null);

            m.Setup(t => t.Read<RolesOnWorkflowsDto>(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException());

            var serverConnection = new ServerConnection(m.Object);

            // Act
            var testDelegate = new TestDelegate(async () => await serverConnection.Login("wrongUsername"));

            // Assert
            Assert.Throws<HttpRequestException>(testDelegate);
            Assert.Fail("Should the exception just propagate further up?" +
                        "Or should it be turned into another exception which is easier to respond to?");
        }

        [Test]
        public async void GetEventsFromWorkflow_Returns_Events()
        {
            // Arrange
            var m = new Mock<HttpClientToolbox>(new Uri("http://someUri/"), null);

            var list = new List<EventAddressDto>
            {
                new EventAddressDto
                {
                    Id = "register",
                    Uri = new Uri("http://localhost:13752")
                },
                new EventAddressDto
                {
                    Id = "pass",
                    Uri = new Uri("http://localhost:13753")
                },
                new EventAddressDto
                {
                    Id = "fail",
                    Uri = new Uri("http://localhost:13754")
                }
            };

            m.Setup(t => t.ReadList<EventAddressDto>(It.IsAny<string>())).ReturnsAsync(list);

            var serverConnection = new ServerConnection(m.Object);

            // Act
            var result = await serverConnection.GetEventsFromWorkflow(new WorkflowDto
            {
                Id = "course",
                Name = "Course workflow"
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public async void GetEventsFromWorkflow_Returns_Empty()
        {
            // Arrange
            var m = new Mock<HttpClientToolbox>(new Uri("http://someUri/"), null);

            m.Setup(t => t.ReadList<EventAddressDto>(It.IsAny<string>())).ReturnsAsync(new List<EventAddressDto>());

            var serverConnection = new ServerConnection(m.Object);

            // Act
            var result = await serverConnection.GetEventsFromWorkflow(new WorkflowDto
            {
                Id = "course",
                Name = "Course workflow"
            });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetEventsFromWorkflow_Throws_Exception()
        {
            // Arrange
            var m = new Mock<HttpClientToolbox>(new Uri("http://someUri/"), null);

            m.Setup(t => t.ReadList<EventAddressDto>(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException());

            var serverConnection = new ServerConnection(m.Object);

            // Act
            var testDelegate = new TestDelegate(async () => await serverConnection.GetEventsFromWorkflow(new WorkflowDto
            {
                Id = "course",
                Name = "Course workflow"
            }));

            // Assert
            Assert.Throws<HttpRequestException>(testDelegate);
            Assert.Fail("Should the exception just propagate further up?" +
                        "Or should it be turned into another exception which is easier to respond to?");
        }
    }
}
