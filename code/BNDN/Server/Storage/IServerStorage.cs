﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Server.Models;

namespace Server.Storage
{
    public interface IServerStorage : IDisposable
    {
        ServerUserModel GetUser(string username);

        ICollection<ServerRoleModel> Login(ServerUserModel userModel);

        /// <summary>
        /// Get all events from a workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        IEnumerable<ServerEventModel> GetEventsFromWorkflow(ServerWorkflowModel workflow);

        Task AddRolesToWorkflow(IEnumerable<ServerRoleModel> roles);

        Task AddUser(ServerUserModel user);

        /// <summary>
        /// Add event to a workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventToBeAddedDto"></param>
        Task AddEventToWorkflow(ServerEventModel eventToBeAddedDto);
        
        /// <summary>
        /// Updates an event.
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventToBeUpdated"></param>
        Task UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated);

        /// <summary>
        /// Remove an event from a workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventId"></param>
        void RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId);

        /// <summary>
        /// Get all workflows
        /// </summary>
        /// <returns></returns>
        ICollection<ServerWorkflowModel> GetAllWorkflows();

        ServerWorkflowModel GetWorkflow(string workflowId);

        /// <summary>
        /// Adds a new workflow
        /// If a worksflot with the same ID exists, it will throw an exception
        /// </summary>
        /// <param name="workflow"></param>
        Task AddNewWorkflow(ServerWorkflowModel workflow);

        Task UpdateWorkflow(ServerWorkflowModel workflow);

        Task RemoveWorkflow(ServerWorkflowModel workflow);

        Task<bool> RoleExists(ServerRoleModel role);

        Task<ServerRoleModel> GetRole(string id, string workflowId);
    }
}
