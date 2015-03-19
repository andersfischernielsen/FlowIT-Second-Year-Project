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
        private static AwiaHttpClientToolbox _awiaHttp;
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
            if (_awiaHttp == null)
            {
                _awiaHttp = new AwiaHttpClientToolbox(uri);
            }
            return _instance;
        }

        public Task<IList<WorkflowDto>> GetWorkflows()
        {
            return _awiaHttp.ReadList<WorkflowDto>("/workflows/");
        }

        public Task<IList<EventAddressDto>> GetEventsFromWorkflow(WorkflowDto workflow)
        {
            return _awiaHttp.ReadList<EventAddressDto>("/workflows/" + workflow);
        }
    }
}
