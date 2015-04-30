using System;
using System.Threading.Tasks;

namespace Event.Interfaces
{
    public interface IEventStorageForReset : IDisposable
    {
        Task ClearLock(string workflowId, string eventId);
        Task ResetToInitialState(string workflowId, string eventId);
        Task<bool> Exists(string workflowId, string eventId);
    }
}
