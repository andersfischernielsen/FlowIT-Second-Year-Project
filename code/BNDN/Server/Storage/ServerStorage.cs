using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Storage
{
    public class ServerStorage : IServerStorage
    {
        private readonly IServerContext _db;

        public ServerStorage(IServerContext context = null)
        {
            _db = context ?? new StorageContext();
        }

        public async Task<ServerUserModel> GetUser(string username)
        {
            return await _db.Users.SingleOrDefaultAsync(user => string.Equals(user.Name, username));
        }

        public async Task<ICollection<ServerRoleModel>> Login(ServerUserModel userModel)
        {
            if (userModel.ServerRolesModels != null) return userModel.ServerRolesModels;

            var user = await _db.Users.FindAsync(userModel.Id);
            return user != null ? user.ServerRolesModels : null;
        }

        public async Task<IEnumerable<ServerEventModel>> GetEventsFromWorkflow(ServerWorkflowModel workflow)
        {
            var events = from e in _db.Events
                         where workflow.Id == e.ServerWorkflowModelId
                         select e;
            return await events.ToListAsync();
        }

        public async Task AddRolesToWorkflow(IEnumerable<ServerRoleModel> roles)
        {
            foreach (var role in roles)
            {
                if (!await RoleExists(role))
                {
                    _db.Roles.Add(role);
                }
            }
            await _db.SaveChangesAsync();
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
            var uu = _db.Users.Create();
            uu.Name = user.Name;
            uu.ServerRolesModels = user.ServerRolesModels;

            _db.Users.Add(uu);

            await _db.SaveChangesAsync();
        }

        public async Task AddEventToWorkflow(ServerEventModel eventToBeAddedDto)
        {
            var workflows = from w in _db.Workflows
                            where eventToBeAddedDto.ServerWorkflowModelId == w.Id
                            select w;

            if (workflows.Count() != 1)
            {
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

        public async Task<ICollection<ServerWorkflowModel>> GetAllWorkflows()
        {
            var workflows = from w in _db.Workflows select w;

            return await workflows.ToListAsync();
        }

        public async Task<ServerWorkflowModel> GetWorkflow(string workflowId)
        {
            var workflows = from w in _db.Workflows
                            where w.Id == workflowId
                            select w;

            return await workflows.SingleAsync();
        }

        public async Task AddNewWorkflow(ServerWorkflowModel workflow)
        {
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
    }
}