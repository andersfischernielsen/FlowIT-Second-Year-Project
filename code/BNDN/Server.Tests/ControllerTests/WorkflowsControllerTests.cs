﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Common;
using Moq;
using NUnit.Framework;
using Server.Controllers;
using Server.Logic;
using Server.Models;

namespace Server.Tests.ControllerTests
{
    [TestFixture]
    class WorkflowsControllerTests
    {
        private Mock<IServerLogic> _mock;

        [SetUp]
        public void SetUp()
        {
            _mock = new Mock<IServerLogic>();
            _mock.Setup(logic => logic.Dispose());
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
            var result = controller.Get().ToList();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.IsNotNull(result[0]);
            Assert.AreEqual("testWorkflow", result[0].Id);
            Assert.AreEqual("Test Workflow", result[0].Name);
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
        public async void PostWorkflowAddsANewWorkflow()
        {
            var list = new List<WorkflowDto>();
            // Arrange
            _mock.Setup(logic => logic.AddNewWorkflow(It.IsAny<WorkflowDto>()))
                .Returns(async (WorkflowDto workflowDto) => list.Add(workflowDto));

            var workflow = new WorkflowDto {Id = "id", Name = "name"};

            var controller = new WorkflowsController(_mock.Object);

            Assert.IsEmpty(list);

            // Act
            await controller.PostWorkFlow(workflow);

            // Assert
            Assert.IsNotEmpty(list);
            Assert.AreEqual(workflow.Id, list.First().Id);
            Assert.AreEqual(workflow.Name, list.First().Name);
        }


        [Test]
        [TestCase("testWorkflow1")]
        [TestCase("IdMedSværeBogstaverÅØOgTegn$")]
        public async void PostWorkflow_id_that_does_not_exist(string workflowId)
        {
            var list = new List<ServerWorkflowModel>();
            _mock.Setup((logic => logic.AddNewWorkflow(It.IsAny<WorkflowDto>())))
                .Returns(async (WorkflowDto incoming) => list.Add(new ServerWorkflowModel {Id = incoming.Id, Name = incoming.Id}));

            // Arrange
            var dto = new WorkflowDto {Id = workflowId, Name = "Workflow Name"};

            var controller = new WorkflowsController(_mock.Object);

            // Act
            await controller.PostWorkFlow(dto);

            // Assert
            Assert.IsNotEmpty(list);
            Assert.IsNotNull(list.First(w => w.Id == workflowId));
        }

        [Test]
        [TestCase("NonexistentWorkflowId")]
        [TestCase("EtAndetWorkflowSomIkkeEksisterer")]
        [TestCase(null)]
        public async void PostWorkflow_id_already_exists(string workflowId)
        {
            //TODO: Michael?
            // Arrange
            var dto = new WorkflowDto { Id = workflowId, Name = "Workflow Name" };

            _mock.Setup(logic => logic.AddNewWorkflow(dto)).Throws<Exception>();

            var controller = new WorkflowsController(_mock.Object);

            try {
                await controller.PostWorkFlow(dto);
            }
            catch (Exception e) {
                Assert.IsInstanceOf<HttpResponseException>(e);
                var ex = (HttpResponseException) e;
                Assert.AreEqual(HttpStatusCode.Conflict, ex.Response.StatusCode);
            }
        }

        [Test]
        [TestCase("AWorkflowId")]
        [TestCase(null)]
        public void PostWorkflow_with_id_and_null_workflow(string workflowId)
        {
            // Arrange
            _mock.Setup(logic => logic.AddNewWorkflow(null)).Throws<ArgumentNullException>();

            var controller = new WorkflowsController(_mock.Object);

            try {
                // Act
                var testDelegate = new TestDelegate(() => controller.PostWorkFlow(null));
            }
            catch (Exception ex) {
                // Assert
                Assert.IsInstanceOf<HttpResponseException>(ex);
                var e = (HttpResponseException) ex;
                Assert.AreEqual(HttpStatusCode.BadRequest, e.Response.StatusCode);
            }
        }

        #endregion

        #region DELETE Workflow

        [Test]
        public void Delete_Workflow_That_Does_Exist(string workflowId)
        {
            //TODO: Make this test not return a weird reflection exception.
            var list = new List<ServerWorkflowModel> { new ServerWorkflowModel { Id = "DoesExist", Name = "This is a test..."} };

            _mock.Setup((logic => logic.RemoveWorkflow(It.IsAny<WorkflowDto>())))
                .Callback(async (WorkflowDto incoming) => list.Remove(list.Find(w => w.Id == incoming.Id)));

            var dto = new WorkflowDto { Id = "DoesExist", Name = "lol"};
            var controller = new WorkflowsController(_mock.Object);

            Assert.DoesNotThrow(async () => controller.DeleteWorkflow(dto.Id));
            Assert.IsEmpty(list.Where(w => w.Id == workflowId));
        }

        [Test]
        public void Delete_Workflow_That_Does_Not_Exist(string workflowId)
        {
            //TODO: Make this test not return a weird reflection exception.
            var list = new List<ServerWorkflowModel> { new ServerWorkflowModel { Id = "DoesNotExist", Name = "This is a test..." } };

            _mock.Setup((logic => logic.RemoveWorkflow(It.IsAny<WorkflowDto>())))
                .Callback(async (WorkflowDto incoming) =>
                {
                    if (list.Count(w => w.Id == incoming.Id) != 0) return;

                    list.Remove(list.Find(w => w.Id == incoming.Id));
                });

            var dto = new WorkflowDto { Id = "SomeDto", Name = "lol" };
            var controller = new WorkflowsController(_mock.Object);

            Assert.DoesNotThrow(async () => controller.DeleteWorkflow(dto.Id));
            Assert.IsNotEmpty(list.Where(w => w.Id == workflowId));
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
            //TODO: Rewrite this test to return a lambda that ensures that the list is empty, then throw the correct exception.
            //TODO: Possibly add the list as an outer variable, and then use that for every test (<- Smart).

            //// Arrange
            //_mock.Setup(logic => logic.GetEventsOnWorkflow(It.IsAny<string>())).Callback(() => { throw new Exception("You dun goofd!"); });

            //var controller = new WorkflowsController(_mock.Object);

            //// Assert
            //var exception = Assert.Throws<HttpResponseException>(() => controller.Get("testWorkflow1"));
            //Assert.AreEqual(HttpStatusCode.BadRequest, exception.Response.StatusCode);
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
