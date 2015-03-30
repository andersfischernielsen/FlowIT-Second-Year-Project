using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Server.Models;

namespace Server.Storage
{
    public class ServerStorage : IServerStorage
    {
        private StorageContext _db;

        public ServerStorage()
        {
            _db = new StorageContext();
        }

        public IEnumerable<ServerEventModel> GetEventsOnWorkflow(ServerWorkflowModel workflow)
        {
            IQueryable<ServerEventModel> events = from e in _db.Events
                where workflow.WorkflowId == e.ServerWorkflowModelId
                select e;

            return events.ToList();
        }

        // TODO: ServerWorkflowModel is unused.
        public void AddEventToWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeAddedDto)
        {
            IQueryable<ServerWorkflowModel> workflows = from w in _db.Workflows
                where eventToBeAddedDto.ServerWorkflowModelId == w.WorkflowId
                select w;

            if (workflows.Count() != 1)
            {
                throw new IOException("Multiple or no workflow with given Id.");
            }

            _db.Events.Add(eventToBeAddedDto);
            _db.SaveChangesAsync();
        }

        public void UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated)
        {
            IQueryable<ServerEventModel> events = from e in _db.Events
                where e.EventId == eventToBeUpdated.EventId
                select e;

            var tempEvent = events.First();
            // TODO: Is it possible to change workflow? 
            tempEvent.ServerWorkflowModel = eventToBeUpdated.ServerWorkflowModel;
            tempEvent.ServerWorkflowModelId = eventToBeUpdated.ServerWorkflowModelId;
            tempEvent.Uri = eventToBeUpdated.Uri;

            _db.SaveChangesAsync();
        }

        public void RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId)
        {
            IQueryable<ServerEventModel> events = from e in _db.Events
                where e.EventId == eventId
                select e;

            _db.Events.Remove(events.First());
            _db.SaveChangesAsync();
        }

        public IEnumerable<ServerWorkflowModel> GetAllWorkflows()
        {
            IQueryable<ServerWorkflowModel> workflows = from w in _db.Workflows select w;

            return workflows.ToList();
        }

        public ServerWorkflowModel GetWorkflow(string workflowId)
        {
            IQueryable<ServerWorkflowModel> workflows = from w in _db.Workflows
                where w.WorkflowId == workflowId
                select w;

            return workflows.First();
        }

        public void AddNewWorkflow(ServerWorkflowModel workflow)
        {
            _db.Workflows.Add(workflow);
            _db.SaveChangesAsync();
        }

        public void UpdateWorkflow(ServerWorkflowModel workflow)
        {
            IQueryable<ServerWorkflowModel> workflows = from w in _db.Workflows
                                                  where w.WorkflowId == workflow.WorkflowId
                                                  select w;

            var tempWorkflow = workflows.First();
            tempWorkflow.Name = workflow.Name;

            _db.SaveChangesAsync();
        }

        public void RemoveWorkflow(ServerWorkflowModel workflow)
        {
            var workflows = 
                from w in _db.Workflows
                where w.WorkflowId == workflow.WorkflowId
                select w;

            _db.Workflows.Remove(workflows.First());
            _db.SaveChangesAsync();
        }
    }
}