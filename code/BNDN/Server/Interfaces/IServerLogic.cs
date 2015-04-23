using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Server.Interfaces
{
    public interface IServerLogic : IDisposable
    {
        /// <summary>
        /// Tries to log in / returns all the roles the user has on all workflows
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<RolesOnWorkflowsDto> Login(string username);

        /// <summary>
        /// Add a new user.
        /// </summary>
        /// <param name="user">The UserDto to use for adding.</param>
        Task AddUser(UserDto user);
        
        /// <summary>
        /// Get all events from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        Task<IEnumerable<EventAddressDto>> GetEventsOnWorkflow(string workflowId);
        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflowToAttachToId"></param>
        /// <param name="eventToBeAddedDto"></param>
        Task AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto);
        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflowToAttachToId"></param>
        /// <param name="eventToBeAddedDto"></param>
        Task UpdateEventOnWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto);
        /// <summary>
        /// Remove an event from a workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventId"></param>
        Task RemoveEventFromWorkflow(string workflowId, string eventId);

        /// <summary>
        /// Get all workflows
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<WorkflowDto>> GetAllWorkflows();
        /// <summary>
        /// Get a workflows
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        Task<WorkflowDto> GetWorkflow(string workflowId);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        Task AddNewWorkflow(WorkflowDto workflow);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        Task UpdateWorkflow(WorkflowDto workflow);
        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflowId"></param>
        Task RemoveWorkflow(string workflowId);
    }
}
