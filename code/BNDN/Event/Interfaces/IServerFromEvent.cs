using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    interface IServerFromEvent : IDisposable
    {
        Task DeleteEventFromServer();
        Task<IEnumerable<EventAddressDto>> PostEventToServer(EventAddressDto dto);
    }
}
