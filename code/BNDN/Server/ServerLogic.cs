using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common;
using Server.Models;
using Server.Storage;

namespace Server
{
    public class ServerLogic : IServerLogic
    {
        private IServerStorage _storage;

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
            return new WorkflowDto()
            {
                Id = workflow.ID, 
                Name = workflow.Name
            };
        }

        public RolesOnWorkflowsDto Login(LoginDto loginDto)
        {
            var user = _storage.GetUser(loginDto.Username);
            var rolesModels = _storage.Login(user);
            var rolesOnWorkflows = new Dictionary<string, IList<string>>();
            foreach (var roleModel in rolesModels)
            {
                IList<string> list;

                if (rolesOnWorkflows.TryGetValue(roleModel.ServerWorklowModelID, out list))
                {
                    list.Add(roleModel.ID);
                }
                else
                {
                    rolesOnWorkflows.Add(roleModel.ServerWorklowModelID, new List<string>{roleModel.ID});
                }
            }
            return new RolesOnWorkflowsDto() { RolesOnWorkflows = rolesOnWorkflows };
        }

        public IEnumerable<EventAddressDto> GetEventsOnWorkflow(string workflowId)
        {
            var workflow = _storage.GetWorkflow(workflowId);

            //TODO: Throw exception if result is null. See tests for this class.
            //TODO: If the workflow exists, return an empty list, otherwise throw NotFoundException.
            return _storage.GetEventsOnWorkflow(workflow).Select(model => new EventAddressDto()
            {
                Id = model.ID,
                Uri = new Uri(model.Uri)
            });
        }

        public void AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var workflow = _storage.GetWorkflow(workflowToAttachToId);
            _storage.AddEventToWorkflow(workflow, new ServerEventModel()
            {
                ID = eventToBeAddedDto.Id,
                Uri = eventToBeAddedDto.Uri.ToString(),
                ServerWorkflowModelID = workflowToAttachToId,
                ServerWorkflowModel = workflow
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

        public void AddNewWorkflow(WorkflowDto workflow)
        {
            _storage.AddNewWorkflow(new ServerWorkflowModel()
            {
                ID = workflow.Id,
                Name = workflow.Name,
            });
        }

        public void UpdateWorkflow(WorkflowDto workflow)
        {
            _storage.UpdateWorkflow(new ServerWorkflowModel()
            {
                ID = workflow.Id,
                Name = workflow.Name,
            });
        }

        public void RemoveWorkflow(WorkflowDto workflow)
        {
            _storage.RemoveWorkflow(new ServerWorkflowModel()
            {
                ID = workflow.Id,
                Name = workflow.Name,
            });
        }
    }
}