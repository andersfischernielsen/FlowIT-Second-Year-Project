using System;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;

namespace Event.Communicators
{
    /// <summary>
    /// EventCommunicator handles the outgoing communication from an Event to another.
    /// </summary>
    public class EventCommunicator : IEventFromEvent
    {
        private readonly HttpClientToolbox _httpClient;

        /// <summary>
        /// Create a new EventCommunicator with no outgoing communication addresses.
        /// </summary>
        public EventCommunicator()
        {
            _httpClient = new HttpClientToolbox();
        }

        /// <summary>
        /// For testing purposes (inject a mocked HttpClientToolbox).
        /// </summary>
        /// <param name="toolbox"> The HttpClientToolbox to use for testing purposes.</param>
        public EventCommunicator(HttpClientToolbox toolbox)
        {
            _httpClient = toolbox;
        }

        public async Task<bool> IsExecuted(Uri targetEventUri, string targetWorkflowId, string targetId, string ownId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            return await _httpClient.Read<bool>(String.Format("events/{0}/{1}/executed/{2}", targetWorkflowId, targetId, ownId));
        }

        public async Task<bool> IsIncluded(Uri targetEventUri, string targetWorkflowId, string targetId, string ownId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            return await _httpClient.Read<bool>(String.Format("events/{0}/{1}/included/{2}", targetWorkflowId, targetId, ownId));
        }

        public async Task SendPending(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            await _httpClient.Update(String.Format("events/{0}/{1}/pending/true", targetWorkflowId, targetId), lockDto);
        }

        public async Task SendIncluded(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            await _httpClient.Update(String.Format("events/{0}/{1}/included/true", targetWorkflowId, targetId), lockDto);
        }

        public async Task SendExcluded(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            await _httpClient.Update(string.Format("events/{0}/{1}/included/false", targetWorkflowId, targetId), lockDto);
        }

        /// <summary>
        /// Tries to lock target event
        /// </summary>
        /// <param name="targetEventUri"></param>
        /// <param name="lockDto"></param>
        /// <param name="targetWorkflowId"></param>
        /// <param name="targetId"></param>
        /// <returns></returns>
        public async Task Lock(Uri targetEventUri, LockDto lockDto, string targetWorkflowId, string targetId)
        {
            long oldTimeout = _httpClient.HttpClient.Timeout.Ticks;
            _httpClient.HttpClient.Timeout = new TimeSpan(0,0,10);
            _httpClient.SetBaseAddress(targetEventUri);
            await _httpClient.Create(String.Format("events/{0}/{1}/lock",targetWorkflowId, targetId), lockDto);
            _httpClient.HttpClient.Timeout = new TimeSpan(oldTimeout);
        }

        /// <summary>
        /// Attempts on unlocking the target Event
        /// </summary>
        /// <returns></returns>
        public async Task Unlock(Uri targetEventUri, string targetWorkflowId, string targetId, string unlockId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            await _httpClient.Delete(String.Format("events/{0}/{1}/lock/{2}", targetWorkflowId, targetId, unlockId));
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}