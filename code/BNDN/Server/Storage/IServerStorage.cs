using System.Collections.Generic;
using Common;

namespace Server.Storage
{
    public interface IServerStorage
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
        IEnumerable<EventAddressDto> GetEventsOnWorkflow(WorkflowDto workflow);
        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflowToAttachToId"></param>
        /// <param name="eventToBeAddedDto"></param>
        void AddEventToWorkflow(WorkflowDto workflow, EventAddressDto eventToBeAddedDto);
        /// <summary>
        /// Remove an event from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventId"></param>
        void RemoveEventFromWorkflow(WorkflowDto workflow, string eventId);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        void AddNewWorkflow(WorkflowDto workflow);
    }
}
