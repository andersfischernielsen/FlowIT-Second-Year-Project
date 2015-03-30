using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Client
{
    public interface IServerConnection
    {
        Task<RolesOnWorkflowsDto> Login(string username);
        Task<IList<WorkflowDto>> GetWorkflows();
        Task<IList<EventAddressDto>> GetEventsFromWorkflow(WorkflowDto workflow);
    }
}
