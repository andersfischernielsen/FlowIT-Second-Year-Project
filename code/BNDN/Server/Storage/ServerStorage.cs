using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Common.DTO.History;
using Common.Exceptions;
using Server.Exceptions;
using Server.Interfaces;
using Server.Models;

namespace Server.Storage
{
    /// <summary>
    /// ServerStorage is the layer that rests on top of the actual database. 
    /// </summary>
    public class ServerStorage : IServerStorage, IServerHistoryStorage
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
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<ServerUserModel> GetUser(string username, string password)
        {
            if (username == null || password == null)
            {
                throw new ArgumentNullException();
            }

            var user = await _db.Users.SingleOrDefaultAsync(u => string.Equals(u.Name, username));

            if (user == null) return null;

            return PasswordHasher.VerifyHashedPassword(password, user.Password) ? user : null;
        }

        /// <summary>
        /// Adds the given roles to the user with username, if the user does not already have them.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="roles">The roles to add to the user.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If username or roles are null.</exception>
        /// <exception cref="NotFoundException">If username does not match a user.</exception>
        public async Task AddRolesToUser(string username, IEnumerable<ServerRoleModel> roles)
        {
            if (username == null || roles == null)
            {
                throw
                    new ArgumentNullException();
            }

            var user = await _db.Users.SingleOrDefaultAsync(u => string.Equals(u.Name, username));

            if (user == null)
            {
                throw new NotFoundException();
            }

            foreach (var role in roles)
            {
                var serverRole = await GetRole(role.Id, role.ServerWorkflowModelId);
                if (!user.ServerRolesModels.Contains(serverRole))
                {
                    user.ServerRolesModels.Add(serverRole);
                }
            }

            await _db.SaveChangesAsync();
        }


        // TODO: Someone else than Morten proof-read this documentation, please!
        /// <summary>
        /// Attempts to login using the provided ServerUserModel
        /// </summary>
        /// <param name="userModel">Represents the user</param>
        /// <returns></returns>
        public async Task<ICollection<ServerRoleModel>> Login(ServerUserModel userModel)
        {
            // TODO: Is this correct / sensible? If I provide a non-null object, I can make this method return whatever role I want it to...
            if (userModel.ServerRolesModels != null) return userModel.ServerRolesModels;

            var user = await _db.Users.FindAsync(userModel.Name);
            return user != null ? user.ServerRolesModels : null;
        }

        /// <summary>
        /// Gets the Events within the specified workflow.
        /// </summary>
        /// <param name="workflowId">The id of the workflow, whose Events are to be returned</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided argument is null</exception>
        /// <exception cref="NotFoundException">Thrown if the workflow does not exist at Server</exception>
        public async Task<IEnumerable<ServerEventModel>> GetEventsFromWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            // Check whether workflow exists
            if (!await WorkflowExists(workflowId))
            {
                throw new NotFoundException();
            }

            var events = from e in _db.Events
                         where workflowId == e.ServerWorkflowModelId
                         select e;
            return await events.ToListAsync();
        }

        /// <summary>
        /// Adds the given roles to a workflow, and returns the server-representation of the added roles.
        /// </summary>
        /// <param name="roles">The roles to add to a workflow.</param>
        /// <returns>A collection of the server-representation of the workflows.</returns>
        public async Task<IEnumerable<ServerRoleModel>> AddRolesToWorkflow(IEnumerable<ServerRoleModel> roles)
        {
            // Result contains the ServerRoleModels as EntityFramework sees them.
            var result = new List<ServerRoleModel>();
            foreach (var role in roles)
            {
                if (!await RoleExists(role))
                {
                    // We add the result of the Add call to result, because we don't want entityFramework
                    // To create another identical role when the roles are added to the events.
                    result.Add(_db.Roles.Add(role));
                }
                else
                {
                    // ReSharper says that the following two statements are required in order to do what we want.
                    var roleId = role.Id;
                    var workflowId = role.ServerWorkflowModelId;
                    // We have to find the server-representation of these roles
                    result.Add(await _db.Roles.SingleAsync(r => r.Id == roleId && r.ServerWorkflowModelId == workflowId));
                }
            }
            await _db.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Get the server-representation of a role
        /// </summary>
        /// <param name="rolename">The name of the role. This is what identifies the role.</param>
        /// <param name="workflowId">The Id of the Workflow the role belongs to.</param>
        /// <returns>If found, the server-representation of the role. null otherwise.</returns>
        public async Task<ServerRoleModel> GetRole(string rolename, string workflowId)
        {
            if (rolename == null || workflowId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await WorkflowExists(workflowId))
            {
                throw new NotFoundException();
            }

            return await _db.Roles.SingleOrDefaultAsync(role => role.Id.Equals(rolename) && role.ServerWorkflowModelId.Equals(workflowId));
        }

        /// <summary>
        /// States whether a given username already exists at Server
        /// </summary>
        /// <param name="username">The username to check for</param>
        /// <returns></returns>
        public async Task<bool> UserExists(string username)
        {
            return await _db.Users.AnyAsync(u => u.Name.Equals(username));
        }

        /// <summary>
        /// States whether the given role exists on the server already or not.
        /// </summary>
        /// <param name="role">The role to test</param>
        /// <returns>True if the role exists on the server, false otherwise.</returns>
        public async Task<bool> RoleExists(ServerRoleModel role)
        {
            if (role == null)
            {
                throw new ArgumentNullException();
            }

            return await _db.Roles.AnyAsync(rr => rr.Id == role.Id
                && rr.ServerWorkflowModelId == role.ServerWorkflowModelId);
        }


        /// <summary>
        /// Adds a user to Server. Server holds a hashed value for the password. 
        /// </summary>
        /// <param name="user">Holds the logininformation about the user</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided input is null</exception>
        /// <exception cref="UserExistsException">Thrown if the user already exists at Server</exception>
        public async Task AddUser(ServerUserModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException();
            }

            if (await UserExists(user.Name))
            {
                throw new UserExistsException();
            }

            user.Password = PasswordHasher.HashPassword(user.Password);

            _db.Users.Add(user);

            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// Adds an Event to a specified workflow. The information needed to identify what workflow, the Event should be added to,
        /// is held within argument. 
        /// </summary>
        /// <param name="eventToBeAddedDto">Holds information about a) the Event, that is to be added and 
        /// b) the workflow, the Event should be added to.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Trhown if the provided argument is null</exception>
        /// <exception cref="NotFoundException">Thrown if the workflow could not be found</exception>
        /// <exception cref="EventExistsException">Thrown if the Event already exists</exception>
        /// <exception cref="IllegalStorageStateException">Thrown if the storage was found to be in an illegal state</exception>
        public async Task AddEventToWorkflow(ServerEventModel eventToBeAddedDto)
        {
            if (eventToBeAddedDto == null)
            {
                throw new ArgumentNullException();
            }

            if (!await WorkflowExists(eventToBeAddedDto.ServerWorkflowModelId))
            {
                throw new NotFoundException();
            }
            if (await EventExists(eventToBeAddedDto.ServerWorkflowModelId, eventToBeAddedDto.Id))
            {
                throw new EventExistsException();
            }

            var workflows = from w in _db.Workflows
                            where eventToBeAddedDto.ServerWorkflowModelId == w.Id
                            select w;

            if (workflows.Count() != 1)
            {
                throw new IllegalStorageStateException();
            }

            _db.Events.Add(eventToBeAddedDto);

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Will delete the Event from the specified workflow
        /// </summary>
        /// <param name="workflowId">Id if the workflow</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if either of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if either the workflow or the event could not be found</exception>
        public async Task RemoveEventFromWorkflow(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await WorkflowExists(workflowId))
            {
                throw new NotFoundException();
            }
            if (!await EventExists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            var events = from e in _db.Events
                         where e.Id == eventId
                         select e;

            var eventToRemove = await events.SingleOrDefaultAsync();

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
        /// Checks whether an Event exists or not in the database
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        public async Task<bool> EventExists(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            // Returns true if any Event matches both the workflowId and the eventId, otherwise false.
            return await _db.Events.AnyAsync(x => x.Id == eventId && x.ServerWorkflowModel.Id == workflowId);
        }

        /// <summary>
        /// Returns the specified workflow. 
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public async Task<ServerWorkflowModel> GetWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await WorkflowExists(workflowId))
            {
                throw new NotFoundException();
            }

            var workflows = from w in _db.Workflows
                            where w.Id == workflowId
                            select w;

            return await workflows.SingleOrDefaultAsync();
        }

        /// <summary>
        /// Adds a new workflow. 
        /// </summary>
        /// <param name="workflowToAdd">Represents the workflow that is to be added</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided ServerWorkflowModel is null</exception>
        /// <exception cref="WorkflowAlreadyExistsException">Thrown if the workflow already exists</exception>
        public async Task AddNewWorkflow(ServerWorkflowModel workflowToAdd)
        {
            if (workflowToAdd == null)
            {
                throw new ArgumentNullException();
            }
            if (await WorkflowExists(workflowToAdd.Id))
            {
                throw new WorkflowAlreadyExistsException();
            }

            _db.Workflows.Add(workflowToAdd);
            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// Updates a workflow
        /// </summary>
        /// <param name="replacingWorkflow">The replacing workflow</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided argument is null</exception>
        /// <exception cref="NotFoundException">Thrown if the workflow was not found</exception>
        public async Task UpdateWorkflow(ServerWorkflowModel replacingWorkflow)
        {
            if (replacingWorkflow == null)
            {
                throw new ArgumentNullException();
            }

            if (!await WorkflowExists(replacingWorkflow.Id))
            {
                throw new NotFoundException();
            }

            var workflows = from w in _db.Workflows
                            where w.Id == replacingWorkflow.Id
                            select w;

            var tempWorkflow = workflows.Single();
            tempWorkflow.Name = replacingWorkflow.Name;

            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// Deletes a workflow
        /// </summary>
        /// <param name="workflowId">Id of the workflow to be deleted</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided argument is null</exception>
        /// <exception cref="NotFoundException">Thrown if the workflow was not found</exception>
        /// <exception cref="IllegalStorageStateException">Thrown if the storage was found to be in an illegal state</exception>
        public async Task RemoveWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await WorkflowExists(workflowId))
            {
                throw new NotFoundException();
            }

            var workflows =
                from w in _db.Workflows
                where w.Id == workflowId
                select w;

            if (workflows.Count() > 1)
            {
                // Because workflowId's are unique identifiers, and hence, there should be only a single element in workflows. 
                throw new IllegalStorageStateException();
            }

            _db.Workflows.Remove(await workflows.SingleAsync());
            await _db.SaveChangesAsync();
        }




        public void Dispose()
        {
            _db.Dispose();
        }

        // TODO: Documentation
        public async Task SaveHistory(HistoryModel toSave)
        {
            if (toSave == null)
            {
                throw new ArgumentNullException();
            }

            if (!await WorkflowExists(toSave.WorkflowId))
            {
                throw new NotFoundException();
            }
            _db.History.Add(toSave);
            await _db.SaveChangesAsync();
        }

        // TODO: Documentation
        public async Task SaveNonWorkflowSpecificHistory(HistoryModel toSave)
        {
            if (toSave == null)
            {
                throw new ArgumentNullException();
            }

            _db.History.Add(toSave);
            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// Returns the history (at Server) for the specified workflow.
        /// </summary>
        /// <param name="workflowId">Id of the workflow, whose history (at Server) is to be obtained</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null</exception>
        /// <exception cref="NotFoundException">Thrown if the workflow could not be found</exception>
        public async Task<IQueryable<HistoryModel>> GetHistoryForWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await WorkflowExists(workflowId))
            {
                throw new NotFoundException();
            }

            return _db.History.Where(h => h.WorkflowId == workflowId);
        }
    }
}