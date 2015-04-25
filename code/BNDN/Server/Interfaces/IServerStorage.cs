using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Models;
using Server.Storage;

namespace Server.Interfaces
{
    public interface IServerStorage : IDisposable, IServerHistoryStorage
    {
        #region Workflow related
        /// <summary>
        /// Get all events from a workflow
        /// </summary>
        /// <param name="workflowId">Id of the workflow, whose events are to be retrieved</param>
        /// <returns></returns>
        Task<IEnumerable<ServerEventModel>> GetEventsFromWorkflow(string workflowId);

        Task<ICollection<ServerRoleModel>> AddRolesToWorkflow(IEnumerable<ServerRoleModel> roles);

        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="eventToBeAddedDto"></param>
        Task AddEventToWorkflow(ServerEventModel eventToBeAddedDto);

        /// <summary>
        /// Updates an event.
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventToBeUpdated"></param>
        Task UpdateEventOnWorkflow(string workflowId, ServerEventModel eventToBeUpdated);

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
        Task<ICollection<ServerWorkflowModel>> GetAllWorkflows();

        Task<bool> WorkflowExists(string workflowId);

        Task<bool> EventExists(string workflowId, string eventId); 

        Task<ServerWorkflowModel> GetWorkflow(string workflowId);

        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        Task AddNewWorkflow(ServerWorkflowModel workflow);

        Task UpdateWorkflow(ServerWorkflowModel workflow);

        Task RemoveWorkflow(string workflowId);

        #endregion

        #region User related
        Task AddUser(ServerUserModel user);
        Task<bool> RoleExists(ServerRoleModel role);
        Task<bool> UserExists(string username);
        Task<ServerRoleModel> GetRole(string id, string workflowId);
        Task<ServerUserModel> GetUser(string username, string password);

        Task<ICollection<ServerRoleModel>> Login(ServerUserModel userModel);
        #endregion


    }
}
