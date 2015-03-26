
using System;
using System.Collections.Generic;
using Common;

namespace Server.Models
{
    public class WorkflowStorage : IServerStorage
    {
        public WorkflowStorage()
        {
            
        }

        public IList<WorkflowDto> GetAllWorkflows()
        {
            // Dummy workflows for now (before deleting: consider if it can be used for testing)
            var dummy1 = new WorkflowDto() {Name = "Pay rent", Id = "pay"};
            var dummy2 = new WorkflowDto() {Name = "How to get good grades", Id = "grades"};
            return new List<WorkflowDto>() {dummy1, dummy2};
        }

        public IList<EventAddressDto> GetEventsOnWorkflow(WorkflowDto workflow)
        {
            throw new NotImplementedException();
        }

        public void AddEventToWorkflow(WorkflowDto workflow, EventAddressDto eventToBeAddedDto)
        {
            throw new NotImplementedException();
        }

        public void RemoveEventFromWorkflow(WorkflowDto workflow, string eventId)
        {
            throw new NotImplementedException();
        }

        public void AddNewWorkflow(WorkflowDto workflow)
        {
            throw new NotImplementedException();
        }

        public IList<EventAddressDto> GetEventsWithinWorkflow(string workflowId)
        {
            switch (workflowId)
            {
                case "Computer":
                    // Dummy data (before deleting: it may be used for testing...?) 
                    var eventA = new EventAddressDto { Id = "Apple", Uri = new Uri("http://www.apple.com") };
                    var eventB = new EventAddressDto { Id = "IBM", Uri = new Uri("http://www.ibm.com") };
                    var eventC = new EventAddressDto { Id = "Sam", Uri = new Uri("http://www.samsung.com") };

                    return new List<EventAddressDto> { eventA, eventB, eventC };
                
                case "Car":
                    // Dummy data (before deleting: it may be used for testing...?) 
                    var eventD = new EventAddressDto { Id = "Opel", Uri = new Uri("http://www.opel.dk") };
                    var eventE = new EventAddressDto { Id = "Ford", Uri = new Uri("http://www.ford.dk") };
                    var eventF = new EventAddressDto { Id = "Nis", Uri = new Uri("http://www.nissan.dk") };

                    return new List<EventAddressDto> {eventD, eventE, eventF};

                default:
                    return new List<EventAddressDto>();
            }
        }
    }
}