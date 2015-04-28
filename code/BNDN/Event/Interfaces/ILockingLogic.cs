using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event.Models;

namespace Event.Interfaces
{
    public interface ILockingLogic : IDisposable
    {
        Task LockSelf(string workflowId, string eventId, LockDto lockDto);
        Task UnlockSelf(string workflowId, string eventId, string callerId);
        Task<bool> LockAllForExecute(string workflowId, string eventId);

        Task<bool> UnlockAllForExecute(string workflowId, string eventId);
        Task<bool> IsAllowedToOperate(string workflowId, string eventId, string callerId);
        Task<bool> LockList(SortedDictionary<int, RelationToOtherEventModel> list, string eventId);
        Task<bool> UnlockList(SortedDictionary<int, RelationToOtherEventModel> list, string eventId);
    }
}
