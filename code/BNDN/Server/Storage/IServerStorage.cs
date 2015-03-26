using System.Collections.Generic;
using Common;
using Server.Models;

namespace Server.Storage
{
    public interface IServerStorage
    {
        
        /// <summary>
        /// Get all events from a workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        IEnumerable<ServerEventModel> GetEventsOnWorkflow(ServerWorkflowModel workflow);
        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventToBeAddedDto"></param>
        void AddEventToWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeAddedDto);
        
        /// <summary>
        /// Updates an event.
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventToBeUpdated"></param>
        void UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated);
        /// <summary>
        /// Remove an event from a workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventId"></param>
        void RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>

        /// <summary>
        /// Get all workflows
        /// </summary>
        /// <returns></returns>
        IEnumerable<ServerWorkflowModel> GetAllWorkflows();
        ServerWorkflowModel GetWorkflow(string workflowId);
        void AddNewWorkflow(ServerWorkflowModel workflow);
        void UpdateWorkflow(ServerWorkflowModel workflow);
        void RemoveWorkflow(ServerWorkflowModel workflow);
    }
}
