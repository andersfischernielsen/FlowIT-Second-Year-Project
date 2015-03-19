using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Client
{
    public interface IServerConnection
    {
        Task<IList<WorkflowDto>> GetWorkflows();
        Task<IList<EventDto>> GetEventsFromWorkflow(WorkflowDto workflow);
    }
}
