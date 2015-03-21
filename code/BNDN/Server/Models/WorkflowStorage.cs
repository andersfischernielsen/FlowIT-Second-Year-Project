
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

        public IEnumerable<WorkflowDto> GetAllWorkflows()
        {
            // Dummy workflows for now (before deleting: consider if it can be used for testing)
            var dummy1 = new WorkflowDto() {Name = "Pay rent"};
            var dummy2 = new WorkflowDto() {Name = "How to get good grades"};
            return new List<WorkflowDto>() {dummy1, dummy2};
        }

        public IEnumerable<EventAddressDto> GetEventsWithinWorkflow(int workflowId)
        {
            switch (workflowId)
            {
                case 1:
                    // Dummy data (before deleting: it may be used for testing...?) 
                    var eventA = new EventAddressDto() { Id = "1", Uri = new Uri("http://www.apple.com") };
                    var eventB = new EventAddressDto() { Id = "2", Uri = new Uri("http://www.ibm.com") };
                    var eventC = new EventAddressDto() { Id = "3", Uri = new Uri("http://www.samsung.com") };

                    return new List<EventAddressDto>() { eventA, eventB, eventC };
                
                case 2:
                    // Dummy data (before deleting: it may be used for testing...?) 
                    var eventD = new EventAddressDto() { Id = "1", Uri = new Uri("http://www.opel.dk") };
                    var eventE = new EventAddressDto() { Id = "2", Uri = new Uri("http://www.ford.dk") };
                    var eventF = new EventAddressDto() { Id = "3", Uri = new Uri("http://www.nissan.dk") };

                    return new List<EventAddressDto>() {eventD, eventE, eventF};

                default:
                    return new List<EventAddressDto>();
            }
        }

        public void AddEventToWorkflow(int workflowToAttachToId, int eventId, EventDto eventToBeAddedDto)
        {
            // Add Event to specified workflow
            throw new System.NotImplementedException();
        }

        public void RemoveEventFromWorkflow(int workflowId, int eventId)
        {
            // Remove event from specified workflow
            throw new System.NotImplementedException();
        }
    }
}