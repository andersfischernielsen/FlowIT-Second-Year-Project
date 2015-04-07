using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                new ServerWorkflowModel(){ Name="TestWorkFlow", ID = "Test1", ServerRolesModels = new List<ServerRoleModel>(), ServerEventModels = new List<ServerEventModel>()}
            };
            _userCache = new HashSet<ServerUserModel>()
            {
                new ServerUserModel{ID = 1,Name="Wind",ServerRolesModels = new List<ServerRoleModel>{new ServerRoleModel(){ID="Teacher", ServerWorkflowModelID = "Test1"},new ServerRoleModel(){ID="Student",ServerWorkflowModelID= "Test1"}}},
                new ServerUserModel{ID = 2, Name="Fischer",ServerRolesModels = new List<ServerRoleModel>{new ServerRoleModel(){ID="Teacher", ServerWorkflowModelID = "Test1"}}}
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

        public ICollection<ServerWorkflowModel> GetAllWorkflows()
        {
            return _cache;
        }

        public ServerWorkflowModel GetWorkflow(string workflowId)
        {
            return _cache.First(model => model.ID == workflowId);
        }

        public ServerUserModel GetUser(string username)
        {
            return _userCache.SingleOrDefault(model => String.Equals(model.Name, username, StringComparison.CurrentCultureIgnoreCase));
        }

        public ICollection<ServerRoleModel> Login(ServerUserModel userModel)
        {
            var singleOrDefault = _userCache.SingleOrDefault(model => model.ID == userModel.ID);
            if (singleOrDefault != null)
                return singleOrDefault.ServerRolesModels;
            return new List<ServerRoleModel>();
        }

        public IEnumerable<ServerEventModel> GetEventsFromWorkflow(ServerWorkflowModel workflow)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.ID == workflow.ID);
            if (serverWorkflowModel != null)
                return serverWorkflowModel.ServerEventModels;
            return new List<ServerEventModel>();
        }

        Task IServerStorage.AddRolesToWorkflow(IEnumerable<ServerRoleModel> roles)
        {
            throw new NotImplementedException();
        }

        public Task AddUser(ServerUserModel user)
        {
            throw new NotImplementedException();
        }

        public void AddRolesToWorkflow(IEnumerable<ServerRoleModel> roles)
        {
            throw new NotImplementedException();
        }

        public void AddEventToWorkflow(ServerEventModel eventToBeAddedDto)
        {
            var serverWorkflowModel = _cache.FirstOrDefault(model => model.ID == eventToBeAddedDto.ServerWorkflowModelID);
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

#pragma warning disable 1998
        public async Task AddNewWorkflow(ServerWorkflowModel workflow)
        {
            var check = _cache.FirstOrDefault(model => model.ID == workflow.ID);
            if (check == null)
            {
                _cache.Add(workflow);
            }
            else throw new Exception("Workflow Already Exists");
        }

        public async Task UpdateWorkflow(ServerWorkflowModel workflow)
        {
            var element = _cache.FirstOrDefault(model => model.ID == workflow.ID);
            if (element != null)
            {
                _cache.Add(workflow);
            }
            else throw new Exception("Workflow not found");
        }

        public async Task RemoveWorkflow(ServerWorkflowModel workflow)
        {
            _cache.Remove(workflow);
        }

        public Task<bool> RoleExists(ServerRoleModel role)
        {
            throw new NotImplementedException();
        }

        public Task<ServerRoleModel> GetRole(string id, string workflowId)
        {
            throw new NotImplementedException();
        }
#pragma warning restore 1998
        public void Dispose()
        {
            // Don't dispose anything.
        }
    }
}
