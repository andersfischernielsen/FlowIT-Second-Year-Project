using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.DTO.Event;
using Common.DTO.Server;
using Common.DTO.Shared;
using Common.Exceptions;

namespace Server.Interfaces
{
    public interface IServerLogic : IDisposable
    {
        /// <summary>
        /// Tries to log in / returns all the roles the user has on all workflows
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        Task<RolesOnWorkflowsDto> Login(LoginDto loginDto);

        /// <summary>
        /// Add a new user.
        /// </summary>
        /// <param name="userDto">The UserDto to use for adding.</param>
        Task AddUser(UserDto userDto);

        /// <summary>
        /// Adds the given roles to the given username, if not already included.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="roles">The roles to add to the user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If the username or the roles are null.</exception>
        /// <exception cref="NotFoundException">If the username does not correspond to a user,
        /// or if a role does not exist at some event in the given workflow.</exception>
        Task AddRolesToUser(string username, IEnumerable<WorkflowRole> roles);
        
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
        /// <param name="workflowId"></param>
        Task RemoveWorkflow(string workflowId);
    }
}
