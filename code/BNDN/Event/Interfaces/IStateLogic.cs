using System;
using System.Threading.Tasks;
using Common;
using Common.DTO.Event;

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
