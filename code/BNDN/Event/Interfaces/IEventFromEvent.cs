using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    interface IEventFromEvent
    {
        Task<EventDto> GetEvent(string eventBaseAddress);
        void PostEventRules(string eventBaseAddress, EventRuleDto rules);
        void UpdateEventRules(string eventBaseAddress, EventRuleDto replacingRules);
        void DeleteEventRules(string eventBaseAddress);
    }
}
