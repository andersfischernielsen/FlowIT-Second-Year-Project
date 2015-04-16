using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventFromEvent : IDisposable
    {
        Task<bool> IsExecuted(Uri targetEventUri, string targetId, string ownId);

        Task<bool> IsIncluded(Uri targetEventUri, string targetId, string ownId);

        Task SendPending(Uri targetEventUri, EventAddressDto lockDto, string targetId);

        Task SendIncluded(Uri targetEventUri, EventAddressDto lockDto, string targetId);

        Task SendExcluded(Uri targetEventUri, EventAddressDto lockDto, string targetId);

        Task Lock(Uri targetEventUri, LockDto lockDto, string targetId);

        Task Unlock(Uri targetEventUri, string targetId, string unlockId);
    }
}
