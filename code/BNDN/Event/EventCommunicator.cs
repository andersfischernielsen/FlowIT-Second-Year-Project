using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;
using Event.Models;

namespace Event
{
    /// <summary>
    /// EventCommunicator handles the outgoing communication from an Event to another.
    /// </summary>
    public class EventCommunicator : IEventFromEvent
    {
        public async Task<bool> IsExecuted(Uri eventUri)
        {
            var httpClient = new AwiaHttpClientToolbox(eventUri);
            return await httpClient.Read<bool>("event/executed");
        }

        public async Task<bool> IsIncluded(Uri eventUri)
        {
            var httpClient = new AwiaHttpClientToolbox(eventUri);
            return await httpClient.Read<bool>("event/included");
        }

        /// <summary>
        /// GetEvent will return a representation of the Event asked for
        /// </summary>
        /// <param name="eventUri">The base-address of the event that is asked for</param>
        /// <returns>A Task object revealing af EventDto object</returns>
        public async Task<EventDto> GetEvent(Uri eventUri)
        {
            var httpClient = new AwiaHttpClientToolbox(eventUri);

            return await httpClient.Read<EventDto>("");
        }


        /// <summary>
        /// PostEventRules will post to another event the rules it need to adopt
        /// </summary>
        /// <param name="eventBaseAddress">The base address of the event whose rules are to be updated</param>
        /// <param name="rules">The rule-set it need to adopt</param>
        /// <param name="ownId">The id of the calling event</param>
        public async Task PostEventRules(Uri eventBaseAddress, EventRuleDto rules, string ownId)
        {
            var httpClient = new AwiaHttpClientToolbox(eventBaseAddress);
            await httpClient.Create(String.Format("event/rules/{0}", ownId), rules);
        }

        /// <summary>
        /// UpdateEventRules will post to another Event the rules it need to update.
        /// </summary>
        /// <param name="eventBaseAddress">The base-address of the Event</param>
        /// <param name="replacingRules">The new (replacing ruleset)</param>
        /// <param name="ownId">The id of the calling event</param>
        // TODO: Will the replacing ruleset contain all rules from this Event (whether or not they all need to be updated) or only those that need to be modified? 
        public async Task UpdateEventRules(Uri eventBaseAddress, EventRuleDto replacingRules, string ownId)
        {
            var httpClient = new AwiaHttpClientToolbox(eventBaseAddress);
            await httpClient.Update(String.Format("event/rules/{0}", ownId), replacingRules);
        }

        /// <summary>
        /// Will issue a Delete call on receiving Event's rules. How the receiving Event handles this call
        /// is an implementation detail 
        /// </summary>
        /// <param name="eventBaseAddress">The base-address of the Event whose rules are to be deleted</param>
        /// <param name="ownId">The id of the calling event</param>
        public async Task DeleteEventRules(Uri eventBaseAddress, string ownId)
        {
            var httpClient = new AwiaHttpClientToolbox(eventBaseAddress);
            await httpClient.Delete(String.Format("event/rules/{0}", ownId));
        }

        public async Task SendNotify(Uri eventUri, IEnumerable<NotifyDto> dtos)
        {
            var httpClient = new AwiaHttpClientToolbox(eventUri);
            await httpClient.Update("event/notify", dtos);
        }
    }
}