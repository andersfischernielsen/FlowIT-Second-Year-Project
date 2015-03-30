using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Microsoft.QualityTools.Testing.Fakes;
using Moq;
using NUnit.Framework;

namespace Client.Tests
{
    [TestFixture]
    public class ServerConnectionTests
    {
        private Mock<HttpClientToolbox> _mock;
        

        [SetUp]
        public void SetUp()
        {
            _mock = new Mock<HttpClientToolbox>();
        }

        [Test]
        // Todo: I cannot mock the HttpClientToolbox. Needs some discussion whether to integration test or make it mockable.
        public async void GetWorkflows_Ok()
        {
            // Arrange
            var mock = Mock.Of<HttpClientToolbox>(toolbox => toolbox.ReadList<WorkflowDto>(It.IsAny<string>()) == Task.Run(() => (IList<WorkflowDto>) new List<WorkflowDto>
                {
                    new WorkflowDto {Id = "workflowId1", Name = "Workflow 1"},
                    new WorkflowDto {Id = "workflowId2", Name = "Workflow 2"}
                }));

            var serverConnection = new ServerConnection(mock);

            // Act
            var workflows = await serverConnection.GetWorkflows();

            // Assert
            Assert.IsNotEmpty(workflows);
        }
    }
}
