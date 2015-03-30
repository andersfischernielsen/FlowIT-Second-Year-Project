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
        private readonly HashSet<ServerUserModel> _userCache;

        private CacheStorage()
        {
            _cache = new HashSet<ServerWorkflowModel>()
            {
                new ServerWorkflowModel(){ Name="TestWorkFlow", ID = "Test1", ServerRolesModels = new List<ServerRolesModel>(), ServerEventModels = new List<ServerEventModel>()}
            };
            _userCache = new HashSet<ServerUserModel>()
            {
                new ServerUserModel(){ID = 1,Name="Wind",ServerRolesModels = new List<ServerRolesModel>{new ServerRolesModel(){ID="Teacher",ServerUserModelID = 1, ServerWorklowModelID = "Test1"},new ServerRolesModel(){ID="Student",ServerUserModelID = 1, ServerWorklowModelID= "Test1"}}},
                new ServerUserModel(){ID = 2, Name="Fischer",ServerRolesModels = new List<ServerRolesModel>{new ServerRolesModel(){ID="Teacher",ServerUserModelID = 2, ServerWorklowModelID = "Test1"}}}
            };
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
            return _cache.First(model => model.ID == workflowId);
        }

        public ServerUserModel GetUser(string username)
        {
            return _userCache.SingleOrDefault(model => model.Name.ToLower() == username.ToLower());
        }

        public ICollection<ServerRolesModel> Login(ServerUserModel userModel)
        {
            var singleOrDefault = _userCache.SingleOrDefault(model => model.ID == userModel.ID);
            if (singleOrDefault != null)
                return singleOrDefault.ServerRolesModels;
            return new List<ServerRolesModel>();
        }

        public IEnumerable<ServerEventModel> GetEventsOnWorkflow(ServerWorkflowModel workflow)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.ID == workflow.ID);
            if (serverWorkflowModel != null)
                return serverWorkflowModel.ServerEventModels;
            return new List<ServerEventModel>();
        }

        public void AddEventToWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeAddedDto)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.ID == workflow.ID);
            if (serverWorkflowModel != null && !serverWorkflowModel.ServerEventModels.Contains(eventToBeAddedDto))
            {
                serverWorkflowModel.ServerEventModels.Add(eventToBeAddedDto);
            }
            else throw new Exception("Event already exists");
        }

        public void UpdateEventOnWorkflow(ServerWorkflowModel workflow, ServerEventModel eventToBeUpdated)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.ID == workflow.ID);
            if (serverWorkflowModel != null)
            {
                var existingElement = serverWorkflowModel.ServerEventModels.First(model => model.ID == eventToBeUpdated.ID); // throws exception if not found.
                var index = serverWorkflowModel.ServerEventModels.ToList().IndexOf(existingElement);
                serverWorkflowModel.ServerEventModels.ToList()[index] = eventToBeUpdated;
            }
            else throw new Exception("Element could not be found");
        }

        public void RemoveEventFromWorkflow(ServerWorkflowModel workflow, string eventId)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.ID == workflow.ID);
            if (serverWorkflowModel != null)
            {
                var elementToDelete = serverWorkflowModel.ServerEventModels.First(model => model.ID == eventId);
                serverWorkflowModel.ServerEventModels.Remove(elementToDelete);
            }
            else throw new Exception("Workflow could not be found");
        }

        public void AddNewWorkflow(ServerWorkflowModel workflow)
        {
            var check = _cache.FirstOrDefault(model => model.ID == workflow.ID);
            if (check == null)
            {
                _cache.Add(workflow);
            }
            else throw new Exception("Workflow Already Exists");
        }

        public void UpdateWorkflow(ServerWorkflowModel workflow)
        {
            var element = _cache.FirstOrDefault(model => model.ID == workflow.ID);
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
