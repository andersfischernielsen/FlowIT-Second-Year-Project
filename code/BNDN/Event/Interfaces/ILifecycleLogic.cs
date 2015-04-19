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
        Task DeleteEvent(string workflowId, string eventId);
        Task ResetEvent(string workflowId, string eventId);
        Task<EventDto> GetEventDto(string workflowId, string eventId);
    }
}
