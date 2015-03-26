using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Server
{
    public interface IServerLogic
    {
        /// <summary>
        /// Get all workflows
        /// </summary>
        /// <returns></returns>
        IEnumerable<WorkflowDto> GetAllWorkflows();
        /// <summary>
        /// Get all events from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        IEnumerable<EventAddressDto> GetEventsWithinWorkflow(string workflowId);
        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflowToAttachToId"></param>
        /// <param name="eventToBeAddedDto"></param>
        void AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto);
        /// <summary>
        /// Remove an event from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventId"></param>
        void RemoveEventFromWorkflow(string workflowId, string eventId);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        void AddNewWorkflow(WorkflowDto workflow);
    }
}
