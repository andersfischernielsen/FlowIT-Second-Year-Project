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
        IEnumerable<WorkflowDto> GetAllWorkflows();
        IEnumerable<EventAddressDto> GetEventsWithinWorkflow(int workflowId);
        void AddEventToWorkflow(int workflowToAttachToId, int eventId, EventDto eventToBeAddedDto);
        void RemoveEventFromWorkflow(int workflowId, int eventId);
    }
}
