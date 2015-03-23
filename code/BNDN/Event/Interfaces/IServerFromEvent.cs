using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    interface IServerFromEvent
    {
        Task<IList<EventAddressDto>> GetWorkFlowEvents(int workflowId);
        void SendHeartbeatToServer();
        Task RequestDeletionOfEventAtServer(int eventToBeDeleted);
    }
}
