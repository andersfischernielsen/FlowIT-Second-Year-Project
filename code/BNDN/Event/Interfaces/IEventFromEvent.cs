using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventFromEvent
    {
        Task<EventDto> GetEvent(Uri eventUri);
        Task PostEventRules(Uri eventUri, EventRuleDto rules, string ownId);
        Task UpdateEventRules(Uri eventUri, EventRuleDto replacingRules, string ownId);
        Task DeleteEventRules(Uri eventUri, string ownId);
        Task SendNotify(Uri eventUri, IEnumerable<NotifyDto> dtos);
    }
}
