using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Exceptions;
using Server.Exceptions;
using Server.Interfaces;
using Server.Models;

namespace Server.Logic
{
    /// <summary>
    /// ServerLogic is a logic-layer that handles logic related to users-, login- and workflow operations.
    /// </summary>
    public class ServerLogic : IServerLogic
    {
        private readonly IServerStorage _storage;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="storage">The storage-layer that supports this class</param>
        public ServerLogic(IServerStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Returns all workflows. 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<WorkflowDto>> GetAllWorkflows()
        {
            var workflows = await _storage.GetAllWorkflows();

            return workflows.Select(model => new WorkflowDto
            {
                Id = model.Id,
                Name = model.Name
            });
        }

        /// <summary>
        /// Returns information about the specified workflow. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow</param>
        /// <returns></returns>
        public async Task<WorkflowDto> GetWorkflow(string workflowId)
        {
            var workflow = await _storage.GetWorkflow(workflowId);

            return new WorkflowDto
            {
                Id = workflow.Id,
                Name = workflow.Name
            };
        }

        /// <summary>
        /// Will the user's roles on all workflows. 
        /// </summary>
        /// <param name="loginDto">The provided login-information about the user</param>
        /// <returns></returns>
        public async Task<RolesOnWorkflowsDto> Login(LoginDto loginDto)
        {
            var user = await _storage.GetUser(loginDto.Username, loginDto.Password);

            if (user == null)
            {
                throw new UnauthorizedException();
            }

            var rolesModels = await _storage.Login(user);
            var rolesOnWorkflows = new Dictionary<string, IList<string>>();

            foreach (var roleModel in rolesModels)
            {
                IList<string> list;

                if (rolesOnWorkflows.TryGetValue(roleModel.ServerWorkflowModelId, out list))
                {
                    list.Add(roleModel.Id);
                }
                else
                {
                    rolesOnWorkflows.Add(roleModel.ServerWorkflowModelId, new List<string> { roleModel.Id });
                }
            }

            return new RolesOnWorkflowsDto { RolesOnWorkflows = rolesOnWorkflows };
        }

        /// <summary>
        /// Will add a user.
        /// </summary>
        /// <param name="dto">Contains information about the user</param>
        /// <returns></returns>
        public async Task AddUser(UserDto dto)
        {
            var user = new ServerUserModel { Name = dto.Name, Password = dto.Password };
            var roles = new List<ServerRoleModel>();

            foreach (var role in dto.Roles)
            {
                var serverRole = new ServerRoleModel { Id = role.Role, ServerWorkflowModelId = role.Workflow };

                if (await _storage.RoleExists(serverRole))
                {
                    roles.Add(await _storage.GetRole(role.Role, role.Workflow));
                }
                else
                {
                    throw new InvalidOperationException("The role does not exist.");
                }
            }

            user.ServerRolesModels = roles;
            await _storage.AddUser(user);
        }

        /// <summary>
        /// Returns a list of Events on the specified workflow.
        /// </summary>
        /// <param name="workflowId">Id of the workflow</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if workflowId is null</exception>
        public async Task<IEnumerable<EventAddressDto>> GetEventsOnWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            var workflow = await _storage.GetWorkflow(workflowId);

            if (workflow == null)
            {
                throw new NotFoundException();
            }

            var dbList = await _storage.GetEventsFromWorkflow(workflow);

            return
                dbList.Select(
                    ev =>
                        new EventAddressDto
                        {
                            Id = ev.Id,
                            Uri = new Uri(ev.Uri),
                            WorkflowId = workflowId,
                            Roles = ev.ServerRolesModels.Select(ro => ro.Id).ToList()
                        });
        }

        /// <summary>
        /// Adds an Event to the specified workflow
        /// </summary>
        /// <param name="workflowToAttachToId">Id of the workflow, that the Event should be added to</param>
        /// <param name="eventToBeAddedDto">Contains information about the Event, that is to be added</param>
        /// <returns></returns>
        public async Task AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var workflow = await _storage.GetWorkflow(workflowToAttachToId);

            // Add roles to the current workflow if they do not exist (the storage method handles the if-part)
            var roles = await _storage.AddRolesToWorkflow(eventToBeAddedDto.Roles.Select(role => new ServerRoleModel
            {
                Id = role,
                ServerWorkflowModelId = workflowToAttachToId
            }));

            await _storage.AddEventToWorkflow(new ServerEventModel
            {
                Id = eventToBeAddedDto.Id,
                Uri = eventToBeAddedDto.Uri.ToString(),
                ServerWorkflowModelId = workflowToAttachToId,
                ServerWorkflowModel = workflow,
                ServerRolesModels = roles
            });
        }

        // TODO: Is this ever used? (Except by a Controller-route, that is never used itself?)
        /// <summary>
        /// Will update a specified Event on a specified workflow
        /// </summary>
        /// <param name="workflowToAttachToId">Id of the target workflow</param>
        /// <param name="eventToBeAddedDto">Updated information about the Event</param>
        /// <returns></returns>
        public async Task UpdateEventOnWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var workflow = await _storage.GetWorkflow(workflowToAttachToId);
            await _storage.UpdateEventOnWorkflow(workflow, new ServerEventModel
            {
                Id = eventToBeAddedDto.Id,
                Uri = eventToBeAddedDto.Uri.ToString(),
                ServerWorkflowModelId = workflowToAttachToId,
                ServerWorkflowModel = workflow
            });
        }

        /// <summary>
        /// Will delete an Event from a specified workflow. 
        /// </summary>
        /// <param name="workflowId">Id of the target workflow</param>
        /// <param name="eventId">Id of the target Event</param>
        /// <returns></returns>
        public async Task RemoveEventFromWorkflow(string workflowId, string eventId)
        {
            var workflow = await _storage.GetWorkflow(workflowId);
            await _storage.RemoveEventFromWorkflow(workflow, eventId);
        }

        /// <summary>
        /// Adds a new workflow
        /// </summary>
        /// <param name="workflow">Contains information about the workflow</param>
        /// <returns></returns>
        public async Task AddNewWorkflow(WorkflowDto workflow)
        {
            if (await _storage.WorkflowExists(workflow.Id))
            {
                throw new WorkflowAlreadyExistsException();
            }

            await _storage.AddNewWorkflow(new ServerWorkflowModel
            {
                Id = workflow.Id,
                Name = workflow.Name,
            });
        }

        // TODO: Is this ever used? Delete?
        /// <summary>
        /// Updates the specified workflow
        /// </summary>
        /// <param name="workflow">Updated information about the workflow</param>
        /// <returns></returns>
        public async Task UpdateWorkflow(WorkflowDto workflow)
        {
            await _storage.UpdateWorkflow(new ServerWorkflowModel
            {
                Id = workflow.Id,
                Name = workflow.Name,
            });
        }


        /// <summary>
        /// Deletes the specified workflow
        /// </summary>
        /// <param name="workflowId">Id of the workflow to be deleted</param>
        /// <returns></returns>
        public async Task RemoveWorkflow(string workflowId)
        {
            await _storage.RemoveWorkflow(await _storage.GetWorkflow(workflowId));
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}