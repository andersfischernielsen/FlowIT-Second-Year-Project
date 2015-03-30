using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        //TODO: Mayby a test to test that no two workflows can have the same ID?

        [Test]
        public void PostWorkflowAddsANewWorkflow()
        {
            var list = new List<WorkflowDto>();
            // Arrange
            _mock.Setup(logic => logic.AddNewWorkflow(It.IsAny<WorkflowDto>()))
                .Callback(((WorkflowDto workflowDto) => list.Add(workflowDto)));

            var workflow = new WorkflowDto() {Id = "id", Name = "name"};

            var controller = new WorkflowsController(_mock.Object);

            // Act
            controller.PostWorkFlow(workflow);

            // Assert
            Assert.AreEqual(workflow, list.First());
        }
        [Test]
        public void PostWorkflowWithNullReturnsXX()
        {
            
            // Arrange
            var list = new List<WorkflowDto>();
            _mock.Setup(logic => logic.AddNewWorkflow(It.IsAny<WorkflowDto>()))
                .Callback(((WorkflowDto workflowDto) => list.Add(workflowDto)));

            WorkflowDto workflow = null;

            var controller = new WorkflowsController(_mock.Object);

            // Assert
            Assert.Throws<Exception>(()=>controller.PostWorkFlow(workflow));
        }

        [Test]
        [TestCase("testWorkflow1")]
        [TestCase("IdMedSværeBogstaverÅØOgTegn$")]
        public void PostWorkflow_id_that_does_not_exist(string workflowId)
        {
            // Arrange
            var dto = new WorkflowDto { Id = workflowId, Name = "Workflow Name" };

            _mock.Setup(logic => logic.AddNewWorkflow(dto)).Verifiable();

            var controller = new WorkflowsController(_mock.Object);

            // Act
            controller.PostWorkFlow(dto);

            // Assert
            Assert.DoesNotThrow(() => _mock.Verify(logic => logic.AddNewWorkflow(dto), Times.Once()));
        }

        [Test]
        [TestCase("NonexistentWorkflowId")]
        [TestCase("EtAndetWorkflowSomIkkeEksisterer")]
        [TestCase(null)]
        public void PostWorkflow_id_already_exists(string workflowId)
        {
            // Arrange
            var dto = new WorkflowDto { Id = workflowId, Name = "Workflow Name" };

            _mock.Setup(logic => logic.AddNewWorkflow(dto)).Throws<Exception>();

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var testDelegate = new TestDelegate(() => controller.PostWorkFlow(dto));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Discuss whether this HttpStatusCode should be used in this case.
            // Todo: Double Assert.
            Assert.AreEqual(HttpStatusCode.Conflict, exception.Response.StatusCode);
        }

        [Test]
        [TestCase("AWorkflowId")]
        [TestCase(null)]
        public void PostWorkflow_with_id_and_null_workflow(string workflowId)
        {
            // Arrange
            _mock.Setup(logic => logic.AddNewWorkflow(null)).Throws<ArgumentNullException>();

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var testDelegate = new TestDelegate(() => controller.PostWorkFlow(null));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Double Assert.
            Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }


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
        public void GetWorkflow_right_list_when_multiple_workflows_exists()
        {
            // Arrange
            _mock.Setup(logic => logic.GetEventsOnWorkflow("id1")).Returns(new List<EventAddressDto> { new EventAddressDto { Id = "id1", Uri = null }});
            _mock.Setup(logic => logic.GetEventsOnWorkflow("id2")).Returns(new List<EventAddressDto>
            {
                new EventAddressDto { Id = "id1", Uri = null },
                new EventAddressDto { Id = "id2", Uri = null }
            });
            var controller = new WorkflowsController(_mock.Object);

            // Act
            var result = controller.Get("id1");

            // Assert
            Assert.AreEqual(1, result.Count());
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
        [Test]
        public void PostEventToWorkflowAddsEventToWorkflow()
        {
            var list = new List<EventAddressDto>();
            // Arrange
            _mock.Setup(logic => logic.AddEventToWorkflow(It.IsAny<string>(), It.IsAny<EventAddressDto>()))
                .Callback(((string s, EventAddressDto eventDto) => list.Add(eventDto)));
            _mock.Setup(logic => logic.GetEventsOnWorkflow(It.IsAny<string>())).Returns(list);

            var eventAddressDto = new EventAddressDto() { Id = "id", Uri = new Uri("http://www.contoso.com/") };

            var controller = new WorkflowsController(_mock.Object);

            // Act
            controller.PostEventToWorkFlow("workflow", eventAddressDto);

            // Assert
            Assert.AreEqual(eventAddressDto, list.First());
        }

        #endregion

        #region PUT Event

        #endregion

        #region DELETE Event
        #endregion
    }
}
