using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Server
{
    interface IServerStorage
    {
        IList<WorkflowDto> GetAllWorkflows();
        IList<EventAddressDto> GetEventsWithinWorkflow(int workflowId);
        void AddEventToWorkflow(int workflowToAttachToId,EventAddressDto eventToBeAddedDto);
        void RemoveEventFromWorkflow(int workflowId, string eventId);
    }
}
