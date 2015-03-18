using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Event.Controllers
{
    interface IServerFromEvent
    {
        Task<IList<EventAddressDto>> GetWorkFlowEvents(int workflowId);
        Task<int> SendHeartbeatToServer();
        void SubmitMyselfToServer();
        Task RequestDeletionOfEventAtServer(int eventToBeDeleted);
    }
}
