using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    public interface IStateLogic : IDisposable
    {
        Task<bool> IsExecuted(string workflowId, string eventId, string senderId);
        Task<bool> IsIncluded(string workflowId, string eventId, string senderId);
        Task<EventStateDto> GetStateDto(string workflowId, string eventId, string senderId);
        Task SetIncluded(string workflowId, string eventId, string senderId, bool newIncludedValue);
        Task SetPending(string workflowId, string eventId, string senderId, bool newPendingValue);
        Task<bool> Execute(string workflowId, string eventId, RoleDto executeDto);

    }
}
