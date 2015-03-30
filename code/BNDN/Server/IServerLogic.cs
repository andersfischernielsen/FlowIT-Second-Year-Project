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
        /// Tries to log in / returns all the roles the user has on all workflows
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        RolesOnWorkflowsDto Login(LoginDto loginDto);
        
        /// <summary>
        /// Get all events from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        IEnumerable<EventAddressDto> GetEventsOnWorkflow(string workflowId);
        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflowToAttachToId"></param>
        /// <param name="eventToBeAddedDto"></param>
        void AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto);
        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflowToAttachToId"></param>
        /// <param name="eventToBeAddedDto"></param>
        void UpdateEventOnWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto);
        /// <summary>
        /// Remove an event from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventId"></param>
        void RemoveEventFromWorkflow(string workflowId, string eventId);

        /// <summary>
        /// Get all workflows
        /// </summary>
        /// <returns></returns>
        IEnumerable<WorkflowDto> GetAllWorkflows();
        /// <summary>
        /// Get a workflows
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        WorkflowDto GetWorkflow(string workflowId);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        void AddNewWorkflow(WorkflowDto workflow);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        void UpdateWorkflow(WorkflowDto workflow);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        void RemoveWorkflow(WorkflowDto workflow);
    }
}
