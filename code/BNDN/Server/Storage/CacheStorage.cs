using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Server.Models;

namespace Server.Storage
{
    public class CacheStorage : IServerStorage
    {
        private static CacheStorage _instance;
        private readonly HashSet<ServerWorkflowModel> _cache;

        private CacheStorage()
        {
            _cache = new HashSet<ServerWorkflowModel>();
        }

        public static CacheStorage GetStorage
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CacheStorage();
                }
                return _instance;
            }
        }

        public IEnumerable<ServerWorkflowModel> GetAllWorkflows()
        {
            return _cache;
        }

        public ServerWorkflowModel GetWorkflow(string workflowId)
        {
            return _cache.FirstOrDefault(model => model.WorkflowId == workflowId);
        }

        public IEnumerable<ServerEventModel> GetEventsOnWorkflow(ServerWorkflowModel workflow)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.WorkflowId == workflow.WorkflowId);
            if (serverWorkflowModel != null)
                return serverWorkflowModel.ServerEventModels;
            return new List<ServerEventModel>();
        }

        public void AddEventToWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeAddedDto)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.WorkflowId == workflow.WorkflowId);
            if (serverWorkflowModel != null && !serverWorkflowModel.ServerEventModels.Contains(eventToBeAddedDto))
            {
                serverWorkflowModel.ServerEventModels.Add(eventToBeAddedDto);
            }
            else throw new Exception("Event already exists");
        }

        public void UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.WorkflowId == workflow.WorkflowId);
            if (serverWorkflowModel != null)
            {
                var existingElement = serverWorkflowModel.ServerEventModels.First(model => model.EventId == eventToBeUpdated.EventId); // throws exception if not found.
                var index = serverWorkflowModel.ServerEventModels.IndexOf(existingElement);
                serverWorkflowModel.ServerEventModels[index] = eventToBeUpdated;
            }
            else throw new Exception("Element could not be found");
        }

        public void RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.WorkflowId == workflow.WorkflowId);
            if (serverWorkflowModel != null)
            {
                var elementToDelete = serverWorkflowModel.ServerEventModels.First(model => model.EventId == eventId);
                serverWorkflowModel.ServerEventModels.Remove(elementToDelete);
            }
            else throw new Exception("Workflow could not be found");
        }

        public void AddNewWorkflow(ServerWorkflowModel workflow)
        {
            var check = _cache.FirstOrDefault(model => model.WorkflowId == workflow.WorkflowId);
            if (check == null)
            {
                _cache.Add(workflow);
            }
            else throw new Exception("Workflow Already Exists");
        }

        public void UpdateWorkflow(ServerWorkflowModel workflow)
        {
            var element = _cache.FirstOrDefault(model => model.WorkflowId == workflow.WorkflowId);
            if (element != null)
            {
                _cache.Add(workflow);
            }
            else throw new Exception("Workflow not found");
        }

        public void RemoveWorkflow(ServerWorkflowModel workflow)
        {
            _cache.Remove(workflow);
        }
    }
}
