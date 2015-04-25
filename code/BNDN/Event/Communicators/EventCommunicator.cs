using System;
using System.Threading.Tasks;
using Common;
using Event.Exceptions;
using Event.Exceptions.EventInteraction;
using Event.Interfaces;
using Event.Models;

namespace Event.Communicators
{
    /// <summary>
    /// EventCommunicator handles the outgoing communication from an Event to another Event.
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
        /// For testing purposes; (inject a mocked HttpClientToolbox).
        /// </summary>
        /// <param name="toolbox"> The HttpClientToolbox to use for testing purposes.</param>
        public EventCommunicator(HttpClientToolbox toolbox)
        {
            _httpClient = toolbox;
        }

        /// <summary>
        /// Asks the specified target Event whether it is executed.
        /// </summary>
        /// <param name="targetEventUri">URI of the target Event, whose Executed value is asked for</param>
        /// <param name="targetWorkflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="targetId">Id of the target Event</param>
        /// <param name="ownId">Id of the calling Event.</param>
        /// <returns></returns>
        /// <exception cref="FailedToGetExecutedFromAnotherEventException">Thrown if method fails to retrieve Executed value from the target Event</exception>
        public async Task<bool> IsExecuted(Uri targetEventUri, string targetWorkflowId, string targetId, string ownId)
        {
            _httpClient.SetBaseAddress(targetEventUri);

            try
            {
                return await _httpClient.Read<bool>(String.Format("events/{0}/{1}/executed/{2}", targetWorkflowId, targetId, ownId));
            }
            catch (Exception)
            {
                throw new FailedToGetExecutedFromAnotherEventException();
            }
            
        }

        /// <summary>
        /// Will determine if the target event is included (true) or not (false). 
        /// </summary>
        /// <param name="targetEventUri">URI of the target Event</param>
        /// <param name="targetWorkflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="targetId">Id of the target Event</param>
        /// <param name="ownId">Id of the calling Event.</param>
        /// <returns></returns>
        /// <exception cref="FailedToGetIncludedFromAnotherEventException">Thrown if Included could not be retrieved from the target Event</exception>
        public async Task<bool> IsIncluded(Uri targetEventUri, string targetWorkflowId, string targetId, string ownId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            try
            {
                return await _httpClient.Read<bool>(String.Format("events/{0}/{1}/included/{2}", targetWorkflowId, targetId, ownId));
            }
            catch (Exception)
            {
                throw new FailedToGetIncludedFromAnotherEventException();
            }
        }

        /// <summary>
        /// SendExcluded attempts on updating the Pending value on the target Event
        /// </summary>
        /// <param name="targetEventUri">URI of the target Event</param>
        /// <param name="lockDto">Should describe caller of this method.</param>
        /// <param name="targetWorkflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="targetId">Id of the target Event</param>
        /// <returns></returns>
        /// <exception cref="FailedToUpdatePendingAtAnotherEventException">Thrown if Pending value failed to be updated at the target Event</exception>
        public async Task SendPending(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            try
            {
                await _httpClient.Update(String.Format("events/{0}/{1}/pending/true", targetWorkflowId, targetId), lockDto);
            }
            catch (Exception)
            {
                throw new FailedToUpdatePendingAtAnotherEventException();
            }
            
        }

        /// <summary>
        /// SendExcluded attempts on updating the Included value on the target Event
        /// </summary>
        /// <param name="targetEventUri">URI of the target Event</param>
        /// <param name="lockDto">Should describe caller of this method.</param>
        /// <param name="targetWorkflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="targetId">Id of the target Event</param>
        /// <returns></returns>
        /// <exception cref="FailedToUpdateIncludedAtAnotherEventException">Thrown if Included value failed to be updated at the target Event</exception>
        public async Task SendIncluded(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            try
            {
                await _httpClient.Update(String.Format("events/{0}/{1}/included/true", targetWorkflowId, targetId), lockDto);
            }
            catch (Exception)
            {
                throw new FailedToUpdateIncludedAtAnotherEventException();
            }
            
        }

        /// <summary>
        /// SendExcluded attempts on updating the Excluded value on the target Event
        /// </summary>
        /// <param name="targetEventUri">URI of the target Event</param>
        /// <param name="lockDto">Contents should describe caller of this method.</param>
        /// <param name="targetWorkflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="targetId">Id of the target Event</param>
        /// <returns></returns>
        /// <exception cref="FailedToUpdateExcludedAtAnotherEventException">Thrown if Excluded value failed to be updated at the target Event</exception>
        public async Task SendExcluded(Uri targetEventUri, EventAddressDto lockDto, string targetWorkflowId, string targetId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            try
            {
                await _httpClient.Update(string.Format("events/{0}/{1}/included/false", targetWorkflowId, targetId), lockDto);
            }
            catch (Exception)
            {
                throw new FailedToUpdateExcludedAtAnotherEventException();
            }  
        }

        /// <summary>
        /// Tries to lock target event
        /// </summary>
        /// <param name="targetEventUri">URI of the target Event</param>
        /// <param name="lockDto">Should describe caller of this method.</param>
        /// <param name="targetWorkflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="targetId">Id of the target Event</param>
        /// <returns></returns>
        /// <exception cref="FailedToLockOtherEventException">Thrown if this method fails to lock the target Event</exception>
        public async Task Lock(Uri targetEventUri, LockDto lockDto, string targetWorkflowId, string targetId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            try
            {
                await _httpClient.Create(String.Format("events/{0}/{1}/lock", targetWorkflowId, targetId), lockDto);
            }
            catch (Exception)
            {
                throw new FailedToLockOtherEventException();
            }
        }

        /// <summary>
        /// Attempts on unlocking the target Event
        /// </summary>
        /// <param name="targetEventUri">URI of the target Event</param>
        /// <param name="targetWorkflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="targetId">Id of the target Event</param>
        /// <param name="unlockId"></param>
        /// <returns></returns>
        /// <exception cref="FailedToUnlockOtherEventException">Thrown if this method fails to unlock the target Event</exception>
        public async Task Unlock(Uri targetEventUri, string targetWorkflowId, string targetId, string unlockId)
        {
            _httpClient.SetBaseAddress(targetEventUri);
            try
            {
                await _httpClient.Delete(String.Format("events/{0}/{1}/lock/{2}", targetWorkflowId, targetId, unlockId));
            }
            catch (Exception)
            {
                throw new FailedToUnlockOtherEventException();
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}