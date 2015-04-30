using System;
using System.Threading.Tasks;
using Common;
using Common.DTO.Shared;

namespace Event.Interfaces
{
    interface IServerFromEvent : IDisposable
    {
        Task DeleteEventFromServer();
        Task PostEventToServer(EventAddressDto dto);
    }
}
