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

        [Test]
        [TestCase("testWorkflow1")]
        [TestCase("IdMedSværeBogstaverÅØOgTegn$")]
        public void PostWorkflow_id_that_does_not_exist(string workflowId)
        {
            // Arrange
            var dto = new WorkflowDto {Id = workflowId, Name = "Workflow Name"};

            _mock.Setup(logic => logic.AddNewWorkflow(dto)).Verifiable();

            var controller = new WorkflowsController(_mock.Object);

            // Act
            controller.PostWorkFlow(workflowId, dto);

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
            var testDelegate = new TestDelegate(() => controller.PostWorkFlow(workflowId, dto));

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
            var testDelegate = new TestDelegate(() => controller.PostWorkFlow(workflowId, null));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Double Assert.
            Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }

        [Test]
        public void PostWorkflow_id_does_not_exist_empty_dto()
        {
            // Arrange
            var controller = new WorkflowsController(_mock.Object);

            // Act
            var testDelegate = new TestDelegate(() => controller.PostWorkFlow("nonexistentId", new WorkflowDto()));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Double assert.
            Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }
        #endregion

        #region PUT Workflow
        [Test]
        public void PutWorkflow_Ok()
        {
            // Arrange
            _mock.Setup(logic => logic.UpdateWorkflow(It.IsAny<WorkflowDto>())).Verifiable();
            var controller = new WorkflowsController(_mock.Object);
            var id = "existingId";
            var dto = new WorkflowDto {Id = id, Name = "Test Workflow"};

            // Act
            controller.UpdateWorkflow(id, dto);

            // Assert
            Assert.DoesNotThrow(() => _mock.Verify(logic => logic.UpdateWorkflow(dto), Times.Once()));
        }

        [Test]
        public void PutWorkflow_Id_does_not_exist()
        {
            // Arrange
            var controller = new WorkflowsController(_mock.Object);
            var id = "notExistingId";
            var dto = new WorkflowDto { Id = id, Name = "Test Workflow" };

            // Act
            var testDelegate = new TestDelegate(() => controller.UpdateWorkflow(id, dto));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: double Assert.
            Assert.AreEqual(HttpStatusCode.NotFound, exception.Response.StatusCode);
        }

        [Test]
        public void PutWorkflow_Id_doesnt_match_dto()
        {
            // Arrange
            var controller = new WorkflowsController(_mock.Object);
            var dto = new WorkflowDto {Id = "idWhichDoesntMatch", Name = "Test Workflow"};
            var workflowId = "SomeOtherId";

            // Act
            var testDelegate = new TestDelegate(() => controller.UpdateWorkflow(workflowId, dto));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Double assert.
            Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }

        [Test]
        public void PutWorkflow_null_dto()
        {
            // Arrange
            var controller = new WorkflowsController(_mock.Object);
            
            // Act
            var testDelegate = new TestDelegate(() => controller.UpdateWorkflow("workflowId", null));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Double assert.
            Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }

        [Test]
        public void PutWorkflow_Name_is_null()
        {
            // Arrange
            var controller = new WorkflowsController(_mock.Object);

            // Act
            var testDelegate =
                new TestDelegate(
                    () => controller.UpdateWorkflow("workflowId", new WorkflowDto {Id = "workflowId", Name = null}));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Double assert.
            Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }
        #endregion

        #region DELETE Workflow
        [Test]
        [TestCase("anExistingEmptyWorkflowId")]
        public void DeleteWorkflow_Ok(string workflowId)
        {
            // Arrange
            _mock.Setup(logic => logic.GetWorkflow(workflowId))
                .Returns(new WorkflowDto {Id = workflowId, Name = "NamE"});
            _mock.Setup(logic => logic.GetEventsOnWorkflow(workflowId)).Returns(new List<EventAddressDto>());
            _mock.Setup(logic => logic.RemoveWorkflow(workflowId)).Verifiable();

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var del = (logic => logic.RemoveWorkflow(workflowId));

            // Assert
            Assert.DoesNotThrow(() => _mock.Verify(del, Times.Once));
        }

        [Test]
        [TestCase("Non-existingWorkflowId")]
        public void DeleteWorkflow_Did_not_exist(string workflowId)
        {
            // Arrange
            _mock.Setup(logic => logic.GetWorkflow(workflowId)).Returns((WorkflowDto) null);
            _mock.Setup(logic => logic.GetEventsOnWorkflow(workflowId)).Throws<ArgumentException>();
            _mock.Setup(logic => logic.RemoveWorkflow(workflowId)).Throws<ArgumentException>();

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var testDelegate = new TestDelegate(() => controller.DeleteWorkflow(workflowId));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Double assertion.
            Assert.AreEqual(HttpStatusCode.NotFound, exception.Response.StatusCode);
        }

        [Test]
        [TestCase("WorkflowIdWithEvents")]
        public void DeleteWorkflow_Not_Empty(string workflowId)
        {
            // Arrange
            _mock.Setup(logic => logic.GetWorkflow(workflowId)).Returns(new WorkflowDto { Id = workflowId, Name = "Test Workflow"});
            _mock.Setup(logic => logic.GetEventsOnWorkflow(workflowId)).Returns(new List<EventAddressDto> { new EventAddressDto { Id = "eventId", Uri = new Uri("http://localhost")} });
            _mock.Setup(logic => logic.RemoveWorkflow(workflowId)).Throws<ArgumentException>();

            var controller = new WorkflowsController(_mock.Object);

            // Act
            var testDelegate = new TestDelegate(() => controller.DeleteWorkflow(workflowId));

            // Assert
            var exception = Assert.Throws<HttpResponseException>(testDelegate);

            // Todo: Double assert.
            Assert.AreEqual(HttpStatusCode.Conflict, exception.Response.StatusCode);
        }
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
