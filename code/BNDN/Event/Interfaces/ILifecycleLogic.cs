using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    interface ILifecycleLogic
    {
        void CreateEvent(EventDto eventDto);
        void DeleteEvent(string eventId);
        void ResetEvent(string eventId);
        EventDto GetEventDto(string eventId);
    }
}
