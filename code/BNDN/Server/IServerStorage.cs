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
        /// <summary>
        /// Get all workflows
        /// </summary>
        /// <returns></returns>
        IList<WorkflowDto> GetAllWorkflows();
        /// <summary>
        /// Get all events from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        IList<EventAddressDto> GetEventsWithinWorkflow(int workflowId);
        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflowToAttachToId"></param>
        /// <param name="eventToBeAddedDto"></param>
        void AddEventToWorkflow(int workflowToAttachToId,EventAddressDto eventToBeAddedDto);
        /// <summary>
        /// Remove an event from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventId"></param>
        void RemoveEventFromWorkflow(int workflowId, string eventId);
    }
}
