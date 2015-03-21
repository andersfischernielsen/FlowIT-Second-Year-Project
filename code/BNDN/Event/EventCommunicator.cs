using System.Threading.Tasks;
using Common;
using Event.Interfaces;

namespace Event
{
    /// <summary>
    /// EventCommunicator handles the outgoing communication from an Event to another.
    /// </summary>
    public class EventCommunicator : IEventFromEvent
    {

        public EventCommunicator()
        {

        }


       
        /// <summary>
        /// GetEvent will return a representation of the Event asked for
        /// </summary>
        /// <param name="eventBaseAddress">The base-address of the event that is asked for</param>
        /// <returns>A Task object revealing af EventDto object</returns>
        public async Task<EventDto> GetEvent(string eventBaseAddress)
        {
            var httpClient = new AwiaHttpClientToolbox(eventBaseAddress);
            // It is assumed that a Get-call on the eventBaseAddress will return a EventDto object
            return await httpClient.Read<EventDto>(eventBaseAddress);
        }


        /// <summary>
        /// PostEventRules will post to another event the rules it need to adopt
        /// </summary>
        /// <param name="eventBaseAddress">The base address of the event whose rules are to be updated</param>
        /// <param name="rules">The rule-set it need to adopt</param>
        public async void PostEventRules(string eventBaseAddress, EventRuleDto rules)
        {
            var httpClient = new AwiaHttpClientToolbox(eventBaseAddress);
            const string rulesPath = "Rules";
            await httpClient.Create(rulesPath, rules);
        }

        /// <summary>
        /// UpdateEventRules will post to another Event the rules it need to update.
        /// </summary>
        /// <param name="eventBaseAddress">The base-address of the Event</param>
        /// <param name="replacingRules">The new (replacing ruleset)</param>
        // TODO: Will the replacing ruleset contain all rules from this Event (whether or not they all need to be updated) or only those that need to be modified? 
        public async void UpdateEventRules(string eventBaseAddress, EventRuleDto replacingRules)
        {
            var httpClient = new AwiaHttpClientToolbox(eventBaseAddress);

            // TODO: At what path are the rules located?
            const  string rulesPath = "Rules/";
            await httpClient.Update(rulesPath, replacingRules);
        }

        /// <summary>
        /// Will issue a Delete call on receiving Event's rules. How the receiving Event handles this call
        /// is an implementation detail 
        /// </summary>
        /// <param name="eventBaseAddress">The base-address of the Event whose rules are to be deleted</param>
        public async void DeleteEventRules(string eventBaseAddress)
        {
            var httpClient = new AwiaHttpClientToolbox(eventBaseAddress);
            const string rulesPath = "Rules/";
            await httpClient.Delete(eventBaseAddress);
        }
    }
}