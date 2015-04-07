using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Server.Models;
using Server.Storage;

namespace Server
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
            return workflows.Select(model => new WorkflowDto()
            {
                Id = model.ID, 
                Name = model.Name
            });
        }

        public WorkflowDto GetWorkflow(string workflowId)
        {
            var workflow = _storage.GetWorkflow(workflowId);
            return new WorkflowDto
            {
                Id = workflow.ID, 
                Name = workflow.Name
            };
        }

        public RolesOnWorkflowsDto Login(string username)
        {
            var user = _storage.GetUser(username);
            if (user == null)
            {
                throw new Exception();
            }
            var rolesModels = _storage.Login(user);
            var rolesOnWorkflows = new Dictionary<string, IList<string>>();
            foreach (var roleModel in rolesModels)
            {
                IList<string> list;

                if (rolesOnWorkflows.TryGetValue(roleModel.ServerWorkflowModelID, out list))
                {
                    list.Add(roleModel.ID);
                }
                else
                {
                    rolesOnWorkflows.Add(roleModel.ServerWorkflowModelID, new List<string>{roleModel.ID});
                }
            }
            return new RolesOnWorkflowsDto { RolesOnWorkflows = rolesOnWorkflows };
        }

        public async Task AddUser(ServerUserModel user)
        {
            await _storage.AddUser(user);
        }

        public IEnumerable<EventAddressDto> GetEventsOnWorkflow(string workflowId)
        {
            var workflow = _storage.GetWorkflow(workflowId);

            //TODO: Throw exception if result is null. See tests for this class.
            //TODO: If the workflow exists, return an empty list, otherwise throw NotFoundException.
            return _storage.GetEventsFromWorkflow(workflow).Select(model => new EventAddressDto
            {
                Id = model.ID,
                Uri = new Uri(model.Uri)
            });
        }

        public void AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var workflow = _storage.GetWorkflow(workflowToAttachToId);

            // Add roles to the current workflow if they do not exist (the storage method handles the if-part)
            _storage.AddRolesToWorkflow(eventToBeAddedDto.Roles.Select(role => new ServerRoleModel
            {
                ID = role,
                ServerWorkflowModelID = workflowToAttachToId
            }));

            _storage.AddEventToWorkflow(new ServerEventModel
            {
                ID = eventToBeAddedDto.Id,
                Uri = eventToBeAddedDto.Uri.ToString(),
                ServerWorkflowModelID = workflowToAttachToId,
                ServerWorkflowModel = workflow,
            });
        }

        public void UpdateEventOnWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var workflow = _storage.GetWorkflow(workflowToAttachToId);
            _storage.UpdateEventOnWorkflow(workflow, new ServerEventModel()
            {
                ID = eventToBeAddedDto.Id,
                Uri = eventToBeAddedDto.Uri.ToString(),
                ServerWorkflowModelID = workflowToAttachToId,
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
            await _storage.AddNewWorkflow(new ServerWorkflowModel()
            {
                ID = workflow.Id,
                Name = workflow.Name,
            });
        }

        public async Task UpdateWorkflow(WorkflowDto workflow)
        {
            await _storage.UpdateWorkflow(new ServerWorkflowModel()
            {
                ID = workflow.Id,
                Name = workflow.Name,
            });
        }

        public async Task RemoveWorkflow(WorkflowDto workflow)
        {
            await _storage.RemoveWorkflow(new ServerWorkflowModel()
            {
                ID = workflow.Id,
                Name = workflow.Name,
            });
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}