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
    public class ServerLogic : IServerLogic
    {
        private readonly IServerStorage _storage;

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
            var user = new ServerUserModel {Name = dto.Name, Password = dto.Password};
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

        public async Task<IEnumerable<EventAddressDto>> GetEventsOnWorkflow(string workflowId)
        {
            var workflow = await _storage.GetWorkflow(workflowId);

            if (workflow == null) {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dbList = await _storage.GetEventsFromWorkflow(workflow);
            var final = new List<EventAddressDto>();
            foreach (var dbItem in dbList)
            {
                var e = new EventAddressDto();
                var roles = new List<string>();
                var workflowModel = dbItem.ServerRolesModels;

                //Extracts the roles to an event from the servereventmodel
                foreach (var w in workflowModel)
                {
                    roles.AddRange(w.ServerUserModels.Select(u => u.Name));
                }

                e.Id = dbItem.Id;
                e.Uri = new Uri(dbItem.Uri);
                e.WorkflowId = workflowId;
                e.Roles = roles;
                final.Add(e);
            }
            return final;
        }

        public async Task AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var workflow = await _storage.GetWorkflow(workflowToAttachToId);

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
            var workflow = await _storage.GetWorkflow(workflowToAttachToId);
            await _storage.UpdateEventOnWorkflow(workflow, new ServerEventModel
            {
                Id = eventToBeAddedDto.Id,
                Uri = eventToBeAddedDto.Uri.ToString(),
                ServerWorkflowModelId = workflowToAttachToId,
                ServerWorkflowModel = workflow
            });
        }

        public async Task RemoveEventFromWorkflow(string workflowId, string eventId)
        {
            var workflow = await _storage.GetWorkflow(workflowId);
            await _storage.RemoveEventFromWorkflow(workflow, eventId);
        }

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

        public async Task UpdateWorkflow(WorkflowDto workflow)
        {
            await _storage.UpdateWorkflow(new ServerWorkflowModel
            {
                Id = workflow.Id,
                Name = workflow.Name,
            });
        }

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