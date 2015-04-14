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
        bool IsExecuted(string eventId, string senderId);
        bool IsIncluded(string eventId, string senderId);
        Task<EventStateDto> GetStateDto(string eventId, string senderId);
        void SetIncluded(string eventId, bool newIncludedValue);
        void SetPending(string eventId, bool newPendingValue);
        void Execute(string eventId);

    }
}
