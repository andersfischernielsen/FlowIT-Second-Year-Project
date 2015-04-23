using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Exceptions;
using Event.Interfaces;
using Event.Models;

namespace Event.Logic
{
    public class LockingLogic : ILockingLogic
    {
        private readonly IEventStorage _storage;
        private readonly IEventFromEvent _eventCommunicator;

        public LockingLogic(IEventStorage storage, IEventFromEvent eventCommunicator)
        {
            _storage = storage;
            _eventCommunicator = eventCommunicator;
        }

        /// <summary>
        /// Attempt to lock the specified Event down
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="lockDto">Content should represent this Event</param>
        /// <returns></returns>
        /// <exception cref="LockedException">Thrown if the specified Event is already locked down</exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task LockSelf(string workflowId, string eventId, LockDto lockDto)
        {
            // Check input
            if (workflowId == null || lockDto == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            // Check if this Event is currently locked
            if (!await IsAllowedToOperate(workflowId, eventId, lockDto.LockOwner))
            {
                throw new LockedException();
            }

            // Checks that the provided lockDto actually has sensible values for its fields.
            if (String.IsNullOrEmpty(lockDto.LockOwner) || String.IsNullOrWhiteSpace(lockDto.LockOwner))
            {
                // Reject request on setting the lockDto
                throw new ArgumentException("lockDto.lockOwner was null");
            }

            lockDto.Id = eventId;
            
            await _storage.SetLock(workflowId, eventId, lockDto.LockOwner);    
        }


        /// <summary>
        /// Tries to unlock the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="callerId">Represents caller</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if either of the input arguments are null</exception>
        /// <exception cref="LockedException">Thrown if the Event is currently locked by someone else</exception>
        public async Task UnlockSelf(string workflowId, string eventId, string callerId)
        {
            if (workflowId == null || eventId == null || callerId == null)
            {
                throw new ArgumentNullException("workflowId");
            }

            if (!await IsAllowedToOperate(workflowId, eventId, callerId))
            {
                throw new LockedException();
            }

            await _storage.ClearLock(workflowId, eventId);
        }

        /// <summary>
        /// LockAll attempts to lockall related Events for the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<bool> LockAll(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            var lockedEvents = new List<RelationToOtherEventModel>();

            // Attempt to lock down related, dependent Events down
            
            // Initiate the lockDto that is to be passed to the other Events
            // identifing this Event as the lockowner
            var lockDto = new LockDto {LockOwner = eventId, Id = eventId};

            // Set this Event's own lockDto (so the Event know for the future that it locked itself down)
            await _storage.SetLock(workflowId, eventId, lockDto.LockOwner);

            // Get dependent events
            var resp = await _storage.GetResponses(workflowId, eventId);
            var incl = await _storage.GetInclusions(workflowId, eventId);
            var excl = await _storage.GetExclusions(workflowId, eventId);            

            // Put into Set to eliminate duplicates.
            var eventsToBeLocked = new HashSet<RelationToOtherEventModel>(
                resp.Concat(
                    incl.Concat(
                        excl))             // We have already locked ourselves, so no need to request that.
                    .Where(relation => relation.WorkflowId != workflowId && relation.EventId != eventId));

            // For every related, dependent Event, attempt to lock it
            foreach (var relation in eventsToBeLocked)
            {
                var toLock = new LockDto {LockOwner = eventId, WorkflowId = relation.WorkflowId, Id = relation.EventId};

                try
                {
                    await _eventCommunicator.Lock(relation.Uri, toLock, relation.WorkflowId, relation.EventId);
                    lockedEvents.Add(relation);
                }
                catch (Exception)
                {
                    // Continue (we won't have added the relation, that failed to lock, to _lockedEvents)
                }

            }

            if (eventsToBeLocked.Count != lockedEvents.Count)
            {
                // TODO: May be an error here, if one list contains this Event itself, while the other does not. 
                await UnlockSome(workflowId, eventId, lockedEvents);

                return false;
            }
            return true;
        }

        /// <summary>
        /// UnlockAll attempts to unlock all related events for the specified Event. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns>False if it fails to unlock other Events</returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NullReferenceException">Thrown if Storage layer returns null-relations.</exception>
        public async Task<bool> UnlockAll(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            // Gather dependent events
            
            var resp = await _storage.GetResponses(workflowId, eventId);          // TODO: What if any (or all) of these return null?
            var incl = await _storage.GetInclusions(workflowId, eventId);
            var excl = await _storage.GetExclusions(workflowId, eventId);
            
            if (resp == null || incl == null || excl == null)
            {
                throw new NullReferenceException("At least one of response-,inclusions or exclusions-relations retrieved from storage was null");
            }
            var eventsToBelocked = resp.Concat(incl.Concat(excl));

            if (eventsToBelocked == null)
            {
                throw new NullReferenceException("eventsToBelocked must not be null");
            }

            // Optimistic approach; assuming every unlocking goes well, everyEventIsUnlocked will go unaffected 
            bool everyEventIsUnlocked = true;

            // Unlock the other Events. 
            foreach (var relation in eventsToBelocked)
            {
                try
                {
                    await _eventCommunicator.Unlock(relation.Uri, relation.WorkflowId, relation.EventId, eventId);
                }
                catch (Exception)
                {
                    // TODO: Find out what to do if you cant even unlock. Even.
                    everyEventIsUnlocked = false;
                }
            }
            // TODO: Shouldn't 
            // Unlock self

            // TODO: Shouldn't we do a IsAllowedToOperate() call before doing this call?
            await _storage.ClearLock(workflowId, eventId);
            
            return everyEventIsUnlocked;
        }

        /// <summary>
        /// Will determine, on basis of the provided arguments, if caller is allowed to operate on the target Event. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="eventId">Id of the target Event</param>
        /// <param name="callerId">Id of the Event, that wishes to operate on the target Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<bool> IsAllowedToOperate(string workflowId, string eventId, string callerId)
        {
            if (workflowId == null || eventId == null || callerId == null)
            {
                throw new ArgumentNullException();
            }

            var lockDto = await _storage.GetLockDto(workflowId, eventId);
            
            if (lockDto == null)
            {   // No lock is set!
                return true;
            }

            return lockDto.LockOwner.Equals(callerId);
        }

        public void Dispose()
        {
            _storage.Dispose();
            _eventCommunicator.Dispose();
        }

        /// <summary>
        /// Attempts to unlock the Events, that are provided in the argument list.  
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the target Event belongs to</param>
        /// <param name="eventId">Id of the target Event</param>
        /// <param name="eventsToBeUnlocked">List specifying which Events are to be unlocked</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        private async Task UnlockSome(string workflowId, string eventId, List<RelationToOtherEventModel> eventsToBeUnlocked)
        {
            if (workflowId == null || eventId == null || eventsToBeUnlocked == null)
            {
                throw new ArgumentNullException();
            }

            // Unlock the other Events. 
            foreach (var relation in eventsToBeUnlocked)
            {
                try
                {
                    await _eventCommunicator.Unlock(relation.Uri, relation.WorkflowId, relation.EventId, eventId);
                }
                catch (Exception)
                {
                    // TODO: Find out what to do if you cant even unlock. Even.
                }
            }
        }
    }
}