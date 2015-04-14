using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    interface IServerFromEvent
    {
        Task DeleteEventFromServer();
        Task<IEnumerable<EventAddressDto>> PostEventToServer(EventAddressDto dto);
    }
}
