using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Client
{
    public class ServerConnection : IServerConnection
    {
        private readonly HttpClientToolbox _http;

        
        public ServerConnection(Uri uri) : this(new HttpClientToolbox(uri))
        {
            
        }

        /// <summary>
        /// For testing purposes (dependency injection of mocked Toolbox.
        /// </summary>
        /// <param name="toolbox"></param>
        public ServerConnection(HttpClientToolbox toolbox)
        {
            _http = toolbox;
        }

        public async Task<RolesOnWorkflowsDto> Login(string username)
        {
            return await _http.Read<RolesOnWorkflowsDto>("login");
        }
        
        public async Task<IList<WorkflowDto>> GetWorkflows()
        {
            return await _http.ReadList<WorkflowDto>("workflows");
        }

        public Task<IList<EventAddressDto>> GetEventsFromWorkflow(WorkflowDto workflow)
        {
            return _http.ReadList<EventAddressDto>("workflows/" + workflow.Id);
        }
    }
}
