using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Common;
using Moq;
using NUnit.Framework;
using Server.Controllers;

namespace Server.Tests
{
    [TestFixture]
    class WorkflowsControllerTests
    {
        private Mock<IServerLogic> _mock;

        [SetUp]
        public void SetUp()
        {
            _mock = new Mock<IServerLogic>();
        }

        #region GET Workflows
        [Test]
        public void GetWorkflows_0_elements()
        {
            // Arrange
            _mock.Setup(logic => logic.GetAllWorkflows()).Returns(new List<WorkflowDto>());

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetWorkflows_1_element()
        {
            _mock.Setup(logic => logic.GetAllWorkflows()).Returns(new List<WorkflowDto>{ new WorkflowDto { Id = "testWorkflow", Name = "Test Workflow"}});

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void GetWorkflows_10_elements()
        {
            // Arrange
            var workflowDtos = new List<WorkflowDto>();
            for (var i = 0; i < 10; i++)
            {
                workflowDtos.Add(new WorkflowDto { Id = string.Format("testWorkflow{0}", i), Name = string.Format("Test Workflow {0}", i)});
            }

            _mock.Setup(logic => logic.GetAllWorkflows()).Returns(workflowDtos);

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.AreEqual(10, result.Count());
        }
        #endregion

        #region POST Workflow
        #endregion

        #region PUT Workflow
        #endregion

        #region DELETE Workflow
        #endregion

        #region GET Workflow/Get Events
        [Test]
        [TestCase("workflowId1", 0)]
        [TestCase("workflowId1", 1)]
        [TestCase("workflowId1", 35)]
        public void Get_workflow_returns_list_of_EventAddressDto(string workflowId, int numberOfEvents)
        {
            // Arrange
            var list = new List<EventAddressDto>();

            for (var i = 0; i < numberOfEvents; i++)
            {
                list.Add(new EventAddressDto { Id = string.Format("event{0}", i), Uri = new Uri(string.Format("http://www.example.com/test{0}", i)) });
            }

            _mock.Setup(logic => logic.GetEventsOnWorkflow(workflowId)).Returns(list);

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var result = controller.Get(workflowId);

            // Assert
            Assert.IsInstanceOf<IEnumerable<EventAddressDto>>(result);

            // Todo: Separate test case (?)
            Assert.AreEqual(numberOfEvents, result.Count());
        }

        [Test]
        public void Get_workflow_not_found()
        {
            // Arrange
            _mock.Setup(logic => logic.GetEventsOnWorkflow(It.IsAny<string>())).Returns(new List<EventAddressDto>());

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var testDelegate = new TestDelegate(() => controller.Get("testWorkflow1"));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);
            Assert.AreEqual(HttpStatusCode.NotFound, exception.Response.StatusCode);
        }
        #endregion

        #region POST Event
        #endregion

        #region PUT Event
        #endregion

        #region DELETE Event
        #endregion
    }
}
