using System;
using System.Collections.Generic;
using Common;
using Server.Interfaces;

namespace Server.Models
{
    public class WorkflowStorage : IServerStorage
    {
        public void RemoveEventFromWorkFlow(int eventId, int workflowId)
        {
            throw new NotImplementedException();
        }

        public List<WorkflowDto> GetAllWorkFlows()
        {
            throw new NotImplementedException();
        }

        public List<EventAddressDto> GetEventsWithinWorkflow(int workflowId)
        {
            throw new NotImplementedException();
        }

        public void AddEventToWorkflow(int workflowToAttachToId, int eventId, EventDto eventToBeAddedDto)
        {
            throw new NotImplementedException();
        }
    }
}