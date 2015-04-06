using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventFromEvent
    {
        Task<bool> IsExecuted();
        Task<bool> IsIncluded();
        Task<EventDto> GetEvent();
        Task PostEventRules(EventRuleDto rules);
        Task UpdateEventRules(EventRuleDto replacingRules);
        Task DeleteEventRules();
        Task SendNotify(IEnumerable<NotifyDto> dtos);
    }
}
