using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common;

namespace Server
{
    public class ServerLogic : IServerLogic
    {
        private IServerStorage _storage;

        public ServerLogic(IServerStorage storage)
        {
            _storage = storage;
        }
        public IList<WorkflowDto> GetAllWorkflows()
        {
            return _storage.GetAllWorkflows();
        }

        public IList<EventAddressDto> GetEventsWithinWorkflow(string workflowId)
        {
            IList<WorkflowDto> l = GetAllWorkflows();
            WorkflowDto w = l.Single(x => x.Id == workflowId);
            var events = _storage.GetEventsOnWorkflow(w);
            return events;
        }

        public void AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var l = GetAllWorkflows();
            var w = l.Single(x => x.Id == workflowToAttachToId);
            _storage.AddEventToWorkflow(w, eventToBeAddedDto);
        }

        public void RemoveEventFromWorkflow(string workflowId, string eventId)
        {
            var l = GetAllWorkflows();
            var w = l.Single(x => x.Id == workflowId);
            _storage.RemoveEventFromWorkflow(w, eventId);
        }

        public void AddNewWorkflow(WorkflowDto workflow)
        {
            _storage.AddNewWorkflow(workflow);
        }
    }
}