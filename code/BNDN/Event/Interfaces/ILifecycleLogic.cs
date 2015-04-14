using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    public interface ILifecycleLogic : IDisposable
    {
        Task CreateEvent(EventDto eventDto, Uri ownUri);
        void DeleteEvent(string eventId);
        void ResetEvent(string eventId);
        EventDto GetEventDto(string eventId);
    }
}
