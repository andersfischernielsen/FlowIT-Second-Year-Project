using System;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventFromEvent : IDisposable
    {
        Task<bool> IsExecuted(Uri targetEventUri, string targetWorkflowId, string targetEventId, string ownId);

        Task<bool> IsIncluded(Uri targetEventUri, string targetWorkflowId, string targetEventId, string ownId);

        Task SendPending(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetEventId);

        Task SendIncluded(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetEventId);

        Task SendExcluded(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetEventId);

        Task Lock(Uri targetEventUri, LockDto lockDto, string targetWorkflowId, string targetEventId);

        Task Unlock(Uri targetEventUri, string targetWorkflowId, string targetEventId, string unlockId);
    }
}
