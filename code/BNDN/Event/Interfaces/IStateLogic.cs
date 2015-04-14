using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    interface IStateLogic
    {
        bool IsExecuted(string eventId);
        bool IsIncluded(string eventId);
        EventStateDto GetStateDto(string eventId);
        void SetIncluded(string eventId, bool newIncludedValue);
        void SetPending(string eventId, bool newPendingValue);
        void Execute(string eventId);

    }
}
