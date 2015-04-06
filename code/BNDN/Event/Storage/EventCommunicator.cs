using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;
using Event.Models;

namespace Event.Storage
{
    /// <summary>
    /// EventCommunicator handles the outgoing communication from an Event to another.
    /// </summary>
    public class EventCommunicator : IEventFromEvent
    {
        private readonly HttpClientToolbox _httpClient;
        private string TargetEventId { get; set; }
        private string OwnEventId { get; set; }

        /// <summary>
        /// Create a new EventCommunicator using the provided Uri.
        /// </summary>
        /// <param name="targetEventUri">The base-address of the Event that this instance is to communicate with.</param>
        /// <param name="targetEventId">The id of the Event, that this EventCommunicator is to communicate with </param>
        /// <param name="ownEventId">The id of this Event</param>
        public EventCommunicator(Uri targetEventUri, string targetEventId, string ownEventId)
        {
            _httpClient = new HttpClientToolbox(targetEventUri);
            TargetEventId = targetEventId;
            OwnEventId = ownEventId;
        }

        /// <summary>
        /// For testing purposes (inject a mocked HttpClientToolbox).
        /// </summary>
        /// <param name="toolbox"> The HttpClientToolbox to use for testing purposes.</param>
        public EventCommunicator(HttpClientToolbox toolbox)
        {
            _httpClient = toolbox;
        }

        public async Task<bool> IsExecuted()
        {
            return await _httpClient.Read<bool>(String.Format("events/{0}/executed/{1}", TargetEventId,OwnEventId));
        }

        public async Task<bool> IsIncluded()
        {
            return await _httpClient.Read<bool>(String.Format("events/{0}included/{1}",TargetEventId,OwnEventId));
        }

        /// <summary>
        /// GetEvent will return a representation of the Event asked for
        /// </summary>
        /// <returns>A Task object revealing af EventDto object</returns>
        public async Task<EventDto> GetEvent()
        {
            return await _httpClient.Read<EventDto>(String.Format("events/{0}",TargetEventId));
        }


        /// <summary>
        /// PostEventRules will post to another event the rules it need to adopt
        /// </summary>
        /// <param name="rules">The rule-set it need to adopt</param>
        public async Task PostEventRules(EventRuleDto rules)
        {
            await _httpClient.Create(String.Format("event/rules/{0}", OwnEventId), rules);
        }
        
        /// <summary>
        /// UpdateEventRules will post to another Event the rules it need to update.
        /// </summary>
        /// <param name="replacingRules">The new (replacing ruleset)</param>
        // TODO: Will the replacing ruleset contain all rules from this Event (whether or not they all need to be updated) or only those that need to be modified? 
        // The definition of the PUT-call is that the state will be set to whatever is received, so every rule
        // must be defined in replacingRules. - Mikael.
        public async Task UpdateEventRules(EventRuleDto replacingRules)
        {
            await _httpClient.Update(String.Format("event/rules/{0}", OwnEventId), replacingRules);
        }

        /// <summary>
        /// Will issue a Delete call on receiving Event's rules. How the receiving Event handles this call
        /// is an implementation detail.
        /// </summary>
        public async Task DeleteEventRules()
        {
            await _httpClient.Delete(String.Format("event/rules/{0}", OwnEventId));
        }

        //TODO: Dont use this.
        public async Task SendNotify(IEnumerable<NotifyDto> dtos)
        {
            await _httpClient.Update("event/notify", dtos);
        }

        public async Task SendPending(bool newPendingValue, EventAddressDto lockDto)
        {
            await _httpClient.Update(String.Format("events/{0}/pending/{1}", TargetEventId,newPendingValue), lockDto);
        }
        public async Task SendIncluded(bool newIncludedValue, EventAddressDto lockDto)
        {
            await _httpClient.Update(String.Format("events/{0}/included/{1}", OwnEventId,newIncludedValue), lockDto);
        }

        /// <summary>
        /// Tries to lock target event
        /// </summary>
        /// <param name="lockDto"></param>
        /// <returns></returns>
        public async Task Lock(LockDto lockDto)
        {
            await _httpClient.Create(String.Format("events/{0}/lock",TargetEventId), lockDto);
        }

        /// <summary>
        /// Attempts on unlocking the target Event
        /// </summary>
        /// <returns></returns>
        public async Task Unlock()
        {
            var unlockId = OwnEventId;
            await _httpClient.Delete(String.Format("events/{0}/lock/{1}",TargetEventId,unlockId));
        }
    }
}