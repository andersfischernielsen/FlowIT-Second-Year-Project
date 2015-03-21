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
        Task PostEventRules(EventRuleDto rules, string ownId);
        Task UpdateEventRules(EventRuleDto replacingRules, string ownId);
        Task DeleteEventRules(string ownId);
        Task SendNotify(IEnumerable<NotifyDto> dtos);
    }
}
