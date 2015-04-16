using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Client.Connections
{
    public interface IServerConnection : IDisposable
    {
        Task<RolesOnWorkflowsDto> Login(string username);
        Task<IList<WorkflowDto>> GetWorkflows();
        Task<IList<EventAddressDto>> GetEventsFromWorkflow(WorkflowDto workflow);
    }
}
