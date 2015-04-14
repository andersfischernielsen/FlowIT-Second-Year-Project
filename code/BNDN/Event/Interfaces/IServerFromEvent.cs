using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    interface IServerFromEvent
    {
        Task<IList<EventAddressDto>> GetWorkFlowEvents(int workflowId);
        Task RequestDeletionOfEventAtServer(string eventToBeDeleted);

        Task DeleteEventFromServer();
        Task<IEnumerable<EventAddressDto>> PostEventToServer(EventAddressDto dto);
    }
}
