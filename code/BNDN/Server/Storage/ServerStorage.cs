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
        private readonly StorageContext _db;

        public ServerStorage()
        {
            _db = new StorageContext();
        }

        public ServerUserModel GetUser(string username)
        {
            return _db.Users.SingleOrDefault(user => string.Equals(user.Name, username));
        }

        public ICollection<ServerRoleModel> Login(ServerUserModel userModel)
        {
            if (userModel.ServerRolesModels == null)
            {
                var user = _db.Users.Find(userModel.ID);
                return user != null ? user.ServerRolesModels : null;
            }
            return userModel.ServerRolesModels;
        }

        public IEnumerable<ServerEventModel> GetEventsFromWorkflow(ServerWorkflowModel workflow)
        {
            var events = from e in _db.Events
                         where workflow.ID == e.ServerWorkflowModelID
                         select e;
            return events.ToList();
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
            return
                await
                    _db.Roles.SingleOrDefaultAsync(
                        role => role.ID.Equals(id)
                            && role.ServerWorkflowModelID.Equals(workflowId));
        }

        public async Task<bool> RoleExists(ServerRoleModel role)
        {
            return await _db.Roles.AnyAsync(rr => rr.ID.Equals(role.ID)
                && rr.ServerWorkflowModelID.Equals(role.ServerWorkflowModelID));
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

        public void AddEventToWorkflow(ServerEventModel eventToBeAddedDto)
        {
            //TODO: Skal 2 events kunne have samme ID?
            var workflows = from w in _db.Workflows
                            where eventToBeAddedDto.ServerWorkflowModelID == w.ID
                            select w;

            if (workflows.Count() != 1)
            {
                throw new IOException("Multiple or no workflow with given ID.");
            }

            _db.Events.Add(eventToBeAddedDto);
            _db.SaveChanges();
        }

        public void UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated)
        {
            var events = from e in _db.Events
                         where e.ID == eventToBeUpdated.ID
                         select e;

            var tempEvent = events.Single();
            // TODO: Is it possible to change workflow? 
            tempEvent.ServerWorkflowModel = eventToBeUpdated.ServerWorkflowModel;
            tempEvent.ServerWorkflowModelID = eventToBeUpdated.ServerWorkflowModelID;
            tempEvent.Uri = eventToBeUpdated.Uri;

            _db.SaveChanges();
        }

        public void RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId)
        {
            var events = from e in _db.Events
                         where e.ID == eventId
                         select e;

            var eventToRemove = events.SingleOrDefault();
            if (eventToRemove == null)
            {
                // Event was already deleted
                return;
            }

            _db.Events.Remove(eventToRemove);
            _db.SaveChanges();
        }

        public ICollection<ServerWorkflowModel> GetAllWorkflows()
        {
            var workflows = from w in _db.Workflows select w;

            return workflows.ToList();
        }

        public ServerWorkflowModel GetWorkflow(string workflowId)
        {
            var workflows = from w in _db.Workflows
                            where w.ID == workflowId
                            select w;

            return workflows.Single();
        }

        public async Task AddNewWorkflow(ServerWorkflowModel workflow)
        {
            //TODO: Skal der tjekkes for om der eksisterer et workflow med samme ID, eller er det okay?
            _db.Workflows.Add(workflow);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateWorkflow(ServerWorkflowModel workflow)
        {
            var workflows = from w in _db.Workflows
                            where w.ID == workflow.ID
                            select w;

            var tempWorkflow = workflows.Single();
            tempWorkflow.Name = workflow.Name;

            await _db.SaveChangesAsync();
        }

        public async Task RemoveWorkflow(ServerWorkflowModel workflow)
        {
            var workflows =
                from w in _db.Workflows
                where w.ID == workflow.ID
                select w;

            if (workflow.ServerEventModels.Count > 0)
            {
                throw new IOException("The workflow contains events");
            }

            _db.Workflows.Remove(workflows.Single());
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}