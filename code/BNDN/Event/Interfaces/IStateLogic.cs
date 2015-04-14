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
        Task<bool> IsExecuted(string eventId, string senderId);
        Task<bool> IsIncluded(string eventId, string senderId);
        Task<EventStateDto> GetStateDto(string eventId, string senderId);
        Task SetIncluded(string eventId, string senderId, bool newIncludedValue);
        Task SetPending(string eventId, string senderId, bool newPendingValue);
        Task<bool> Execute(string eventId, ExecuteDto executeDto);

    }
}
