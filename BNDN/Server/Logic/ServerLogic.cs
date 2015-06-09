﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.DTO.Event;
using Common.DTO.Server;
using Common.DTO.Shared;
using Common.Exceptions;
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

        public async Task<IEnumerable<WorkflowDto>> GetAllWorkflows()
        {
            var workflows = await _storage.GetAllWorkflows();

            return workflows.Select(model => new WorkflowDto
            {
                Id = model.Id,
                Name = model.Name
            });
        }

        public async Task<WorkflowDto> GetWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            var workflow = await _storage.GetWorkflow(workflowId);

            return new WorkflowDto
            {
                Id = workflow.Id,
                Name = workflow.Name
            };
        }


        public async Task<RolesOnWorkflowsDto> Login(LoginDto loginDto)
        {
            var user = await _storage.GetUser(loginDto.Username, loginDto.Password);

            if (user == null)
            {
                throw new UnauthorizedException();
            }

            var rolesModels = await _storage.Login(user);
            var rolesOnWorkflows = new Dictionary<string, ICollection<string>>();

            foreach (var roleModel in rolesModels)
            {
                ICollection<string> list;

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

        public async Task AddUser(UserDto userDto)
        {
            if (userDto == null)
            {
                throw new ArgumentNullException();
            }

            var user = new ServerUserModel { Name = userDto.Name, Password = userDto.Password };
            var roles = new List<ServerRoleModel>();

            foreach (var role in userDto.Roles)
            {
                var serverRole = new ServerRoleModel { Id = role.Role, ServerWorkflowModelId = role.Workflow };

                if (await _storage.RoleExists(serverRole))
                {
                    roles.Add(await _storage.GetRole(role.Role, role.Workflow));
                }
                else
                {
                    throw new NotFoundException();
                }
            }
            user.ServerRolesModels = roles;
            await _storage.AddUser(user);
        }

        public async Task AddRolesToUser(string username, IEnumerable<WorkflowRole> roles)
        {
            if (username == null || roles == null)
            {
                throw new ArgumentNullException();
            }

            if (!await _storage.UserExists(username))
            {
                throw new NotFoundException();
            }
            
            var serverRoles = new List<ServerRoleModel>();

            foreach (var serverRoleModel in roles.Select(workflowRole => new ServerRoleModel
            {
                Id = workflowRole.Role,
                ServerWorkflowModelId = workflowRole.Workflow
            }))
            {
                if (await _storage.RoleExists(serverRoleModel))
                {
                    serverRoles.Add(serverRoleModel);
                }
                else
                {
                    throw new NotFoundException();
                }
            }

            await _storage.AddRolesToUser(username, serverRoles);
        }

        public async Task<IEnumerable<EventAddressDto>> GetEventsOnWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            var dbList = await _storage.GetEventsFromWorkflow(workflowId);

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

        public async Task AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            if (workflowToAttachToId == null || eventToBeAddedDto == null)
            {
                throw new ArgumentNullException();
            }

            var workflow = await _storage.GetWorkflow(workflowToAttachToId);

            // Add roles to the current workflow if they do not exist (the storage method handles the if-part)
            var roles = (await _storage.AddRolesToWorkflow(eventToBeAddedDto.Roles.Select(role => new ServerRoleModel
            {
                Id = role,
                ServerWorkflowModelId = workflowToAttachToId
            }))).ToList();

            await _storage.AddEventToWorkflow(new ServerEventModel
            {
                Id = eventToBeAddedDto.Id,
                Uri = eventToBeAddedDto.Uri.ToString(),
                ServerWorkflowModelId = workflowToAttachToId,
                ServerWorkflowModel = workflow,
                ServerRolesModels = roles
            });
        }

        public async Task RemoveEventFromWorkflow(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }
            await _storage.RemoveEventFromWorkflow(workflowId, eventId);
        }

        public async Task AddNewWorkflow(WorkflowDto workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException();
            }

            await _storage.AddNewWorkflow(new ServerWorkflowModel
            {
                Id = workflow.Id,
                Name = workflow.Name,
            });
        }

        public async Task RemoveWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }
            await _storage.RemoveWorkflow(workflowId);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _storage.Dispose();
            }
        }
    }
}