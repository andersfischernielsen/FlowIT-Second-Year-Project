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

        public ServerUserModel GetUser(string username)
        {
            throw new NotImplementedException();
        }

        public ICollection<ServerRolesModel> Login(ServerUserModel userModel)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ServerEventModel> GetEventsOnWorkflow(ServerWorkflowModel workflow)
        {
            IQueryable<ServerEventModel> events = from e in _db.Events
                where workflow.ID == e.ServerWorkflowModelID
                select e;

            return events.ToList();
        }

        // TODO: ServerWorkflowModel is unused.
        public void AddEventToWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeAddedDto)
        {
            IQueryable<ServerWorkflowModel> workflows = from w in _db.Workflows
                where eventToBeAddedDto.ServerWorkflowModelID == w.ID
                select w;

            if (workflows.Count() != 1)
            {
                throw new IOException("Multiple or no workflow with given ID.");
            }

            _db.Events.Add(eventToBeAddedDto);
            _db.SaveChangesAsync();
        }

        public void UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated)
        {
            IQueryable<ServerEventModel> events = from e in _db.Events
                where e.ID == eventToBeUpdated.ID
                select e;

            var tempEvent = events.First();
            // TODO: Is it possible to change workflow? 
            tempEvent.ServerWorkflowModel = eventToBeUpdated.ServerWorkflowModel;
            tempEvent.ServerWorkflowModelID = eventToBeUpdated.ServerWorkflowModelID;
            tempEvent.Uri = eventToBeUpdated.Uri;

            _db.SaveChangesAsync();
        }

        public void RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId)
        {
            IQueryable<ServerEventModel> events = from e in _db.Events
                where e.ID == eventId
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
                where w.ID == workflowId
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
                                                  where w.ID == workflow.ID
                                                  select w;

            var tempWorkflow = workflows.First();
            tempWorkflow.Name = workflow.Name;

            _db.SaveChangesAsync();
        }

        public void RemoveWorkflow(ServerWorkflowModel workflow)
        {
            var workflows = 
                from w in _db.Workflows
                where w.ID == workflow.ID
                select w;

            _db.Workflows.Remove(workflows.First());
            _db.SaveChangesAsync();
        }
    }
}