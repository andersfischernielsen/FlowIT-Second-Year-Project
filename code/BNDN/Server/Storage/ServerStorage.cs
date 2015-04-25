using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.History;
using Server.Interfaces;
using Server.Models;

namespace Server.Storage
{
    /// <summary>
    /// ServerStorage is the layer that rests on top of the actual database. 
    /// </summary>
    public class ServerStorage : IServerStorage
    {
        private readonly IServerContext _db;

        public ServerStorage(IServerContext context = null)
        {
            _db = context ?? new StorageContext();
        }

        /// <summary>
        /// Returns the user, if he/she exists, and the provided password matches. 
        /// Returns null if no user is found. 
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="password">Claimed password of the user</param>
        /// <returns></returns>
        public async Task<ServerUserModel> GetUser(string username, string password)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => string.Equals(u.Name, username));

            if (user == null) return null;

            return PasswordHasher.VerifyHashedPassword(password, user.Password) ? user : null;
        }

        /// <summary>
        /// Attempts to login using the provided ServerUserModel
        /// </summary>
        /// <param name="userModel">Represents the user</param>
        /// <returns></returns>
        public async Task<ICollection<ServerRoleModel>> Login(ServerUserModel userModel)
        {
            if (userModel.ServerRolesModels != null) return userModel.ServerRolesModels;

            var user = await _db.Users.FindAsync(userModel.Id);
            return user != null ? user.ServerRolesModels : null;
        }

        // TODO: Discuss: Could this method not instead just be called with: string workflowId?
        /// <summary>
        /// Gets the Events within the specified workflow.
        /// </summary>
        /// <param name="workflow">Represents the workflow, whose Events are to be returned</param>
        /// <returns></returns>
        public async Task<IEnumerable<ServerEventModel>> GetEventsFromWorkflow(ServerWorkflowModel workflow)
        {
            var events = from e in _db.Events
                         where workflow.Id == e.ServerWorkflowModelId
                         select e;
            return await events.ToListAsync();
        }


        public async Task<ICollection<ServerRoleModel>> AddRolesToWorkflow(IEnumerable<ServerRoleModel> roles)
        {
            var result = new List<ServerRoleModel>();
            foreach (var role in roles)
            {
                if (!await RoleExists(role))
                {
                    result.Add(_db.Roles.Add(role));
                }
                else
                {
                    var roleId = role.Id;
                    result.Add(await _db.Roles.SingleAsync(r => r.Id == roleId));
                }
            }
            await _db.SaveChangesAsync();
            return result;
        }


        public async Task<ServerRoleModel> GetRole(string id, string workflowId)
        {
            return await _db.Roles.SingleOrDefaultAsync(role => role.Id.Equals(id) && role.ServerWorkflowModelId.Equals(workflowId));
        }



        public async Task<bool> RoleExists(ServerRoleModel role)
        {
            return await _db.Roles.AnyAsync(rr => rr.Id.Equals(role.Id)
                && rr.ServerWorkflowModelId.Equals(role.ServerWorkflowModelId));
        }


        public async Task AddUser(ServerUserModel user)
        {
            if (_db.Users.Any(u => u.Name.Equals(user.Name)))
            {
                throw new ArgumentException("User already exists", "user");
            }

            user.Password = PasswordHasher.HashPassword(user.Password);

            _db.Users.Add(user);

            await _db.SaveChangesAsync();
        }


        public async Task AddEventToWorkflow(ServerEventModel eventToBeAddedDto)
        {
            var workflows = from w in _db.Workflows
                            where eventToBeAddedDto.ServerWorkflowModelId == w.Id
                            select w;

            if (workflows.Count() != 1)
            {
                // TODO: Throw IllegalstorageStateException instead (move from Event into Common)
                throw new IOException("Multiple or no workflow with given ID.");
            }

            _db.Events.Add(eventToBeAddedDto);
            await _db.SaveChangesAsync();
        }


        public async Task UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated)
        {
            var events = from e in _db.Events
                         where e.Id == eventToBeUpdated.Id
                         select e;

            var tempEvent = events.Single();
            tempEvent.ServerWorkflowModel = eventToBeUpdated.ServerWorkflowModel;
            tempEvent.ServerWorkflowModelId = eventToBeUpdated.ServerWorkflowModelId;
            tempEvent.Uri = eventToBeUpdated.Uri;

            await _db.SaveChangesAsync();
        }


        public async Task RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId)
        {
            var events = from e in _db.Events
                         where e.Id == eventId
                         select e;

            var eventToRemove = await events.SingleOrDefaultAsync();
            if (eventToRemove == null)
            {
                // Event was already deleted
                return;
            }

            _db.Events.Remove(eventToRemove);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Returns all workflows held in storage. 
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<ServerWorkflowModel>> GetAllWorkflows()
        {
            var workflows = from w in _db.Workflows select w;

            return await workflows.ToListAsync();
        }

        /// <summary>
        /// Determines whether a workflow exists in storage
        /// </summary>
        /// <param name="workflowId">Id of the workflow</param>
        /// <returns></returns>
        public async Task<bool> WorkflowExists(string workflowId)
        {
            return await _db.Workflows.AnyAsync(workflow => workflow.Id == workflowId);
        }

        /// <summary>
        /// Returns the specified workflow. 
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public async Task<ServerWorkflowModel> GetWorkflow(string workflowId)
        {
            var workflows = from w in _db.Workflows
                            where w.Id == workflowId
                            select w;

            return await workflows.SingleOrDefaultAsync();
        }

        /// <summary>
        /// Adds a new workflow. 
        /// </summary>
        /// <param name="workflow">Represents the workflow that is to be added</param>
        /// <returns></returns>
        public async Task AddNewWorkflow(ServerWorkflowModel workflow)
        {
            // TODO: What if that workflow already exists?
            _db.Workflows.Add(workflow);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateWorkflow(ServerWorkflowModel workflow)
        {
            var workflows = from w in _db.Workflows
                            where w.Id == workflow.Id
                            select w;

            var tempWorkflow = workflows.Single();
            tempWorkflow.Name = workflow.Name;

            await _db.SaveChangesAsync();
        }

        public async Task RemoveWorkflow(ServerWorkflowModel workflow)
        {
            var workflows =
                from w in _db.Workflows
                where w.Id == workflow.Id
                select w;

            if (workflow.ServerEventModels.Count > 0)
            {
                throw new IOException("The workflow contains events");
            }

            _db.Workflows.Remove(await workflows.SingleAsync());
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public async Task SaveHistory(HistoryModel toSave)
        {
            if (!await Exists(toSave.WorkflowId))
            {
                throw new InvalidOperationException("The workflowId does not exist");
            }
            _db.History.Add(toSave);
            await _db.SaveChangesAsync();
        }

        public async Task SaveNonWorkflowSpecificHistory(HistoryModel toSave)
        {
            _db.History.Add(toSave);
            await _db.SaveChangesAsync();
        }

        public async Task<IQueryable<HistoryModel>> GetHistoryForWorkflow(string workflowId)
        {
            if (!await Exists(workflowId))
            {
                throw new InvalidOperationException("The workflowId does not exist");
            }

            return _db.History.Where(h => h.WorkflowId == workflowId);
        }

        /// <summary>
        /// Determines whether a workflow already exists
        /// </summary>
        /// <param name="workflowId">The workflow to test for existence</param>
        /// <returns></returns>
        private async Task<bool> Exists(string workflowId)
        {
            return await _db.Workflows.AnyAsync(w => w.Id == workflowId);
        }
    }
}