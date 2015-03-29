using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Server.Models;
using Server.Storage;

namespace Server.Tests
{
    [TestFixture]
    public class ServerStorageTest
    {

        [SetUp]
        public void Setup()
        {
            var w1 = new ServerWorkflowModel {Name = "w1", WorkflowId = "1"};
            var w2 = new ServerWorkflowModel { Name = "w2", WorkflowId = "2" };
            var list = new List<ServerWorkflowModel> { w1, w2 };
            var mock = new Mock<IServerStorage>();

            mock.Setup(m => m.AddEventToWorkflow(It.IsAny<ServerWorkflowModel>(), It.IsAny<ServerEventModel>()))
                .Callback((ServerWorkflowModel workFlowToAddTo, ServerEventModel eventToAdd) => list.Find(e => e.Name == workFlowToAddTo.Name).ServerEventModels.Add(eventToAdd));

            mock.Setup(m => m.AddNewWorkflow(It.IsAny<ServerWorkflowModel>()))
                .Callback((ServerWorkflowModel toAdd) => list.Add(toAdd));

            mock.Setup(m => m.GetAllWorkflows())
                .Returns(list);

            mock.Setup(m => m.GetEventsOnWorkflow(It.IsAny<ServerWorkflowModel>()))
                .Returns((ServerWorkflowModel toGet) => toGet.ServerEventModels);

            mock.Setup(m => m.GetWorkflow(It.IsAny<string>()))
                .Returns((string workflowId) => list.Find(x => x.WorkflowId == (workflowId)));

            mock.Setup(m => m.RemoveEventFromWorkflow(It.IsAny<ServerWorkflowModel>(), It.IsAny<string>()))
                .Callback((ServerWorkflowModel toRemoveFrom, string eventId) =>
                {
                    var events = list.Find(x => x.WorkflowId == toRemoveFrom.WorkflowId).ServerEventModels;
                    var toRemove = events.First(x => x.EventId == eventId);
                    events.Remove(toRemove);
                });
            
            mock.Setup(m => m.RemoveWorkflow(It.IsAny<ServerWorkflowModel>()))
                .Callback((ServerWorkflowModel toRemove) => list.Remove(toRemove));

            mock.Setup(m => m.UpdateEventOnWorkflow(It.IsAny<ServerWorkflowModel>(), It.IsAny<ServerEventModel>()))
                .Callback((ServerWorkflowModel toUpdateOn, ServerEventModel eventToUpdate) =>
                {
                    var events = list.Find(x => x.WorkflowId == toUpdateOn.WorkflowId).ServerEventModels;
                    var toRemove = events.First(x => x.EventId == eventToUpdate.EventId);
                    var index = events.IndexOf(toRemove);
                    events.Insert(index, eventToUpdate);
                });

            mock.Setup(m => m.UpdateWorkflow(It.IsAny<ServerWorkflowModel>()))
                .Callback((ServerWorkflowModel toUpdate) =>
                {
                    var oldWorkflow = list.Find(x => x.WorkflowId == toUpdate.WorkflowId);
                    var index = list.IndexOf(oldWorkflow);
                    list.Insert(index, toUpdate);
                });
        }


        [Test]
        public void TestMethod1()
        {

        }
    }
}
