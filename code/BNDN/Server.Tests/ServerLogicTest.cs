using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Common;
using Moq;
using NUnit.Framework;
using Server.Models;
using Server.Storage;

namespace Server.Tests
{
    [TestFixture]
    public class ServerLogicTest
    {
        private List<ServerWorkflowModel> _list;
        private Mock _mock;
        private ServerLogic _toTest;

        /// <summary>
        /// Set up a Mock IServerStorage for validating logic.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            //Create dummy objects.
            var w1 = new ServerWorkflowModel {Name = "w1", WorkflowId = "1"};
            var w2 = new ServerWorkflowModel { Name = "w2", WorkflowId = "2" };
            _list = new List<ServerWorkflowModel> { w1, w2 };
            var toSetup = new Mock<IServerStorage>();

            //Set up method for adding events to workflows. The callback adds the input parameters to the list.
            toSetup.Setup(m => m.AddEventToWorkflow(It.IsAny<ServerWorkflowModel>(), It.IsAny<ServerEventModel>()))
                .Callback((ServerWorkflowModel workFlowToAddTo, ServerEventModel eventToAdd) => _list.Find(e => e.Name == workFlowToAddTo.Name).ServerEventModels.Add(eventToAdd));

            //Set up method for adding a new workflow. The callback adds the input parameter to the list.
            toSetup.Setup(m => m.AddNewWorkflow(It.IsAny<ServerWorkflowModel>()))
                .Callback((ServerWorkflowModel toAdd) => _list.Add(toAdd));

            //Set up method for getting all workflows. Simply returns the dummy list.
            toSetup.Setup(m => m.GetAllWorkflows())
                .Returns(_list);

            //Set up method for getting all events in a workflow. Gets the list of events on the given workflow.
            toSetup.Setup(m => m.GetEventsOnWorkflow(It.IsAny<ServerWorkflowModel>()))
                .Returns((ServerWorkflowModel toGet) => toGet.ServerEventModels);

            //Set up method for getting a specific workflow. Finds the given workflow in the list.
            toSetup.Setup(m => m.GetWorkflow(It.IsAny<string>()))
                .Returns((string workflowId) => _list.Find(x => x.WorkflowId == (workflowId)));

            //Set up method for removing an event from a workflow. 
            //Finds the given workflow in the list, finds the event in the workflow and removes it.
            toSetup.Setup(m => m.RemoveEventFromWorkflow(It.IsAny<ServerWorkflowModel>(), It.IsAny<string>()))
                .Callback((ServerWorkflowModel toRemoveFrom, string eventId) =>
                {
                    var events = _list.Find(x => x.WorkflowId == toRemoveFrom.WorkflowId).ServerEventModels;
                    var toRemove = events.First(x => x.EventId == eventId);
                    events.Remove(toRemove);
                });
            
            //Set up method for removing workflow. Removes the input workflow from the list. 
            toSetup.Setup(m => m.RemoveWorkflow(It.IsAny<ServerWorkflowModel>()))
                .Callback((ServerWorkflowModel toRemove) => _list.Remove(toRemove));

            //Set up method for updating an event in a workflow.
            //Finds the workflow in the list, finds the event in the workflow and replaces it with the new event.
            toSetup.Setup(m => m.UpdateEventOnWorkflow(It.IsAny<ServerWorkflowModel>(), It.IsAny<ServerEventModel>()))
                .Callback((ServerWorkflowModel toUpdateOn, ServerEventModel eventToUpdate) =>
                {
                    var events = _list.Find(x => x.WorkflowId == toUpdateOn.WorkflowId).ServerEventModels;
                    var toReplace = events.First(x => x.EventId == eventToUpdate.EventId);
                    var index = events.IndexOf(toReplace);
                    events.Insert(index, eventToUpdate);
                });

            //Set up method for updating a workflow.
            //Finds the workflow to update in the list, then replaces it with the new workflow.
            toSetup.Setup(m => m.UpdateWorkflow(It.IsAny<ServerWorkflowModel>()))
                .Callback((ServerWorkflowModel toUpdate) =>
                {
                    var oldWorkflow = _list.Find(x => x.WorkflowId == toUpdate.WorkflowId);
                    var index = _list.IndexOf(oldWorkflow);
                    _list.Insert(index, toUpdate);
                });

            //Assigns the mock to the global variable. 
            //Mock.Setup() is not supported if the variable is already global.
            _mock = toSetup;
            _toTest = new ServerLogic((IServerStorage)_mock.Object);
        }


        [Test]
        public void TestAddEventToWorkflow()
        {
            _toTest.AddEventToWorkflow("1", new EventAddressDto { Id="3", Uri = new Uri("http://1.1.1.1/") });

            var workflow = _list.First(x => x.WorkflowId == "1");
            var expectedEvent = workflow.ServerEventModels.First(x => x.EventId == "3");

            Assert.AreEqual(expectedEvent.EventId, "3");
            Assert.AreEqual(expectedEvent.Uri.AbsoluteUri, "http://1.1.1.1/");
        }

        [Test]
        public void TestAddNewWorkflow()
        {
            _toTest.AddNewWorkflow(new WorkflowDto { Id = "3", Name = "w3"});

            var expectedWorkflow = _list.Find(x => x.WorkflowId == "3");
            Assert.IsNotNull(expectedWorkflow);
            Assert.AreEqual(expectedWorkflow.Name, "w3");
            Assert.AreEqual(expectedWorkflow.WorkflowId, "3");
        }

        [Test]
        public void TestGetAllWorkflows()
        {
            var expected = _toTest.GetAllWorkflows().ToList();

            var w1 = new WorkflowDto {Id = "1", Name = "w1"};
            var w2 = new WorkflowDto {Id = "2", Name = "w2"};

            var exp1 = expected.First(x => x.Id == "1");
            var exp2 = expected.First(x => x.Id == "2");

            Assert.IsNotNull(exp1);
            Assert.IsNotNull(exp2);
            Assert.AreEqual(w1.Id, exp1.Id);
            Assert.AreEqual(w2.Name, exp2.Name);
        }
    }
}
