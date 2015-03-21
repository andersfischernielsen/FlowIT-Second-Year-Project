using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Server.Interfaces
{
    interface IServerStorage
    {
        void RemoveEventFromWorkFlow(int eventId, int workflowId);
        List<WorkflowDto> GetAllWorkFlows();
        List<EventAddressDto> GetEventsWithinWorkflow(int workflowId);
        void AddEventToWorkflow(int workflowToAttachToId, int eventId, EventDto eventToBeAddedDto);
    }
}
