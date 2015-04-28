using System;
using System.Threading.Tasks;
using Event.Models;

namespace Event.Interfaces
{
    public interface ILockingLogic : IDisposable
    {
        Task LockSelf(string workflowId, string eventId, LockDto lockDto);
        Task UnlockSelf(string workflowId, string eventId, string callerId);
        Task<bool> LockAll(string workflowId, string eventId);

        Task<bool> UnlockAll(string workflowId, string eventId);
        Task<bool> IsAllowedToOperate(string workflowId, string eventId, string callerId);
    }
}
