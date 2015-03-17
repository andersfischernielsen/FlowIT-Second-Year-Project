using System.Collections.Generic;
using Common;

namespace Client
{
    public interface IServerConnection
    {
        IList<WorkflowDto> GetWorkflows();
        IList<EventAddressDto> GetEventsFromWorkflow(WorkflowDto workflow);
    }
}
