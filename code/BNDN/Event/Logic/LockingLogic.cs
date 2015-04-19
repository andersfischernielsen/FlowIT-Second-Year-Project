using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Event.Exceptions;
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

        public async Task LockSelf(string workflowId, string eventId, LockDto lockDto)
        {
            // Check input
            if (workflowId == null)
            {
                throw new ArgumentNullException("workflowId");
            }
            if (lockDto == null)
            {
                throw new ArgumentNullException("lockDto", "was null");
            }
            if (eventId == null)
            {
                throw new ArgumentNullException("eventId", "was null");
            }

            // Check if this Event is currently locked
            if (!await IsAllowedToOperate(workflowId, eventId, lockDto.LockOwner))
            {
                // TODO: Throw more relevant exception
                throw new ApplicationException();
            }

            // Checks that the provided lockDto actually has sensible values for its fields.
            if (String.IsNullOrEmpty(lockDto.LockOwner) || String.IsNullOrWhiteSpace(lockDto.LockOwner))
            {
                // Reject request on setting the lockDto
                throw new ArgumentNullException("lockDto.lockOwner", "was null");
            }

            lockDto.Id = eventId;
            await _storage.SetLockDto(workflowId, eventId, lockDto);
        }

        public async Task UnlockSelf(string workflowId, string eventId, string callerId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException("workflowId");
            }
            if (eventId == null)
            {
                throw new ArgumentNullException("callerId","callerId was null");
            }
            if (callerId == null)
            {
                throw new ArgumentNullException("eventId","eventId was null");
            }
            if (!await IsAllowedToOperate(workflowId, eventId, callerId))
            {
                throw new LockedException();
            }
            await _storage.ClearLock(workflowId, eventId);
        }

        public async Task<bool> LockAll(string workflowId, string eventId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException("workflowId");
            }
            if (eventId == null)
            {
                throw  new ArgumentNullException("eventId");
            }

            var eventsToBeLocked = new List<RelationToOtherEventModel>();
            var lockedEvents = new List<RelationToOtherEventModel>();

            // Attempt to lock down related, dependent Events down
            
            // Initiate the lockDto that is to be passed to the other Events
            // identifing this Event as the lockowner
            var lockDto = new LockDto {LockOwner = eventId, Id = eventId};

            // Set this Event's own lockDto (so the Event know for the future that it locked itself down)
            await _storage.SetLockDto(workflowId, eventId, lockDto);


            // Get dependent events
            var resp = _storage.GetResponses(workflowId, eventId);
            var incl = _storage.GetInclusions(workflowId, eventId);
            var excl = _storage.GetExclusions(workflowId, eventId);
            var eventsToBelocked = resp.Concat(incl.Concat(excl));

            // For every related, dependent Event, attempt to lock it
            foreach (var relation in eventsToBelocked)
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
        /// 
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventId"></param>
        /// <returns>False if it fails to unlock other Events</returns>
        public async Task<bool> UnlockAll(string workflowId, string eventId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException("workflowId");
            }
            if (eventId == null)
            {
                throw new ArgumentNullException("eventId","provided eventId was null");
            }

            // Gather dependent events
            var resp = _storage.GetResponses(workflowId, eventId);          // TODO: What if any (or all) of these return null?
            var incl = _storage.GetInclusions(workflowId, eventId);
            var excl = _storage.GetExclusions(workflowId, eventId);
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

            return everyEventIsUnlocked;
        }

        public async Task<bool> IsAllowedToOperate(string workflowId, string eventId, string callerId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException("workflowId");
            }
            if (eventId == null)
            {
                throw new ArgumentNullException("eventId","eventId was null");
            }
            if (callerId == null)
            {
                throw new ArgumentNullException("callerId","caller id was null");
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

        private async Task UnlockSome(string workflowId, string eventId, List<RelationToOtherEventModel> eventsToBeUnlocked)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException("workflowId");
            }
            if (eventId == null)
            {
                throw new ArgumentNullException("eventId","eventId was null");
            }
            if (eventsToBeUnlocked == null)
            {
                throw new ArgumentNullException("eventsToBeUnlocked", "eventsToBeUnlocked was null");
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