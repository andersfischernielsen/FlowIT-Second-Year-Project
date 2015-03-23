using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client
{
    public class ServerConnection : IServerConnection
    {
        private static HttpClientToolbox _http;
        private static ServerConnection _instance;

        private ServerConnection()
        {
            
        }

        public static ServerConnection GetStorage(Uri uri)
        {
            if (_instance == null)
            {
                _instance = new ServerConnection();
            }
            if (_http == null)
            {
                _http = new HttpClientToolbox(uri);
            }
            return _instance;
        }

        public async Task<IList<WorkflowDto>> GetWorkflows()
        {
            return await _http.ReadList<WorkflowDto>("workflows");
        }

        public Task<IList<EventAddressDto>> GetEventsFromWorkflow(WorkflowDto workflow)
        {
            return _http.ReadList<EventAddressDto>("workflows/" + workflow.Name);
        }
    }
}
