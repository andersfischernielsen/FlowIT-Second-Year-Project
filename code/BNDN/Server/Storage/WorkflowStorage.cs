using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Server.Models;

namespace Server.Storage
{
    public class WorkflowStorage : IServerStorage
    {
        public WorkflowStorage()
        {
            
        }

        public ServerUserModel GetUser(string username)
        {
            throw new NotImplementedException();
        }

        public ICollection<ServerRolesModel> Login(ServerUserModel userModel)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServerEventModel> GetEventsOnWorkflow(ServerWorkflowModel workflow)
        {
            switch (workflow.ID)
            {
                case "computer":
                    // Dummy data (before deleting: it may be used for testing...?) 
                   // var eventA = new ServerEventModel { ID = "Apple", Uri = new Uri("http://www.apple.com") };
                    //var eventB = new ServerEventModel { ID = "IBM", Uri = new Uri("http://www.ibm.com") };
                    //var eventC = new ServerEventModel { ID = "Sam", Uri = new Uri("http://www.samsung.com") };

                    //return new List<ServerEventModel> { eventA, eventB, eventC };

                case "car":
                    // Dummy data (before deleting: it may be used for testing...?) 
                    //var eventD = new ServerEventModel { ID = "Opel", Uri = new Uri("http://www.opel.dk") };
                    //var eventE = new ServerEventModel { ID = "Ford", Uri = new Uri("http://www.ford.dk") };
                    //var eventF = new ServerEventModel { ID = "Nis", Uri = new Uri("http://www.nissan.dk") };

                   // return new List<ServerEventModel> { eventD, eventE, eventF };

                default:
                    return new List<ServerEventModel>();
            }
        }

        public void AddEventToWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeAddedDto)
        {
            throw new NotImplementedException();
        }

        public void UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated)
        {
            throw new NotImplementedException();
        }

        public void RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServerWorkflowModel> GetAllWorkflows()
        {
            // Dummy workflows for now (before deleting: consider if it can be used for testing)
            var dummy1 = new ServerWorkflowModel() { Name = "Pay rent", ID = "computer" };
            var dummy2 = new ServerWorkflowModel() { Name = "How to get good grades", ID = "car" };
            return new List<ServerWorkflowModel>() { dummy1, dummy2 };
        }

        public ServerWorkflowModel GetWorkflow(string workflowId)
        {
            var dummy1 = new ServerWorkflowModel() { Name = "Pay rent", ID = "computer" };
            var dummy2 = new ServerWorkflowModel() { Name = "How to get good grades", ID = "car" };
            return new List<ServerWorkflowModel>() { dummy1, dummy2 }.First(model => model.ID == workflowId);
        }

        public void AddNewWorkflow(ServerWorkflowModel workflow)
        {
            throw new NotImplementedException();
        }

        public void UpdateWorkflow(ServerWorkflowModel workflow)
        {
            throw new NotImplementedException();
        }

        public void RemoveWorkflow(ServerWorkflowModel workflow)
        {
            throw new NotImplementedException();
        }
    }
}