﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Server.Models;
using Server.Storage;

namespace Server.Logic
{
    public class ServerLogic : IServerLogic
    {
        private readonly IServerStorage _storage;

        public ServerLogic(IServerStorage storage)
        {
            _storage = storage;
        }

        public IEnumerable<WorkflowDto> GetAllWorkflows()
        {
            var workflows = _storage.GetAllWorkflows();

            return workflows.Select(model => new WorkflowDto
            {
                Id = model.Id, 
                Name = model.Name
            });
        }

        public WorkflowDto GetWorkflow(string workflowId)
        {
            var workflow = _storage.GetWorkflow(workflowId);

            return new WorkflowDto
            {
                Id = workflow.Id, 
                Name = workflow.Name
            };
        }

        public RolesOnWorkflowsDto Login(string username)
        {
            var user = _storage.GetUser(username);

            if (user == null)
            {
                throw new Exception("User was not found.");
            }

            var rolesModels = _storage.Login(user);
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
                    rolesOnWorkflows.Add(roleModel.ServerWorkflowModelId, new List<string>{roleModel.Id});
                }
            }

            return new RolesOnWorkflowsDto { RolesOnWorkflows = rolesOnWorkflows };
        }

        public async Task AddUser(UserDto dto)
        {
            var user = new ServerUserModel {Name = dto.Name};
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

        public IEnumerable<EventAddressDto> GetEventsOnWorkflow(string workflowId)
        {
            var workflow = _storage.GetWorkflow(workflowId);

            if (workflow == null) {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return _storage.GetEventsFromWorkflow(workflow).Select(model => new EventAddressDto
            {
                Id = model.Id,
                Uri = new Uri(model.Uri)
            });
        }

        public async Task AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var workflow = _storage.GetWorkflow(workflowToAttachToId);

            // Add roles to the current workflow if they do not exist (the storage method handles the if-part)
            await _storage.AddRolesToWorkflow(eventToBeAddedDto.Roles.Select(role => new ServerRoleModel
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
            });
        }

        public async Task UpdateEventOnWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var workflow = _storage.GetWorkflow(workflowToAttachToId);
            await _storage.UpdateEventOnWorkflow(workflow, new ServerEventModel
            {
                Id = eventToBeAddedDto.Id,
                Uri = eventToBeAddedDto.Uri.ToString(),
                ServerWorkflowModelId = workflowToAttachToId,
                ServerWorkflowModel = workflow
            });
        }

        public void RemoveEventFromWorkflow(string workflowId, string eventId)
        {
            var workflow = _storage.GetWorkflow(workflowId);
            _storage.RemoveEventFromWorkflow(workflow, eventId);
        }

        public async Task AddNewWorkflow(WorkflowDto workflow)
        {
            await _storage.AddNewWorkflow(new ServerWorkflowModel
            {
                Id = workflow.Id,
                Name = workflow.Name,
            });
        }

        public async Task UpdateWorkflow(WorkflowDto workflow)
        {
            await _storage.UpdateWorkflow(new ServerWorkflowModel
            {
                Id = workflow.Id,
                Name = workflow.Name,
            });
        }

        public async Task RemoveWorkflow(WorkflowDto workflow)
        {
            await _storage.RemoveWorkflow(new ServerWorkflowModel
            {
                Id = workflow.Id,
                Name = workflow.Name,
            });
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}