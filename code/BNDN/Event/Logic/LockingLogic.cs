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
                throw new ArgumentException("lockDto.lockOwner was null");
            }

            lockDto.Id = eventId;
            await _storage.SetLock(workflowId, eventId, lockDto.LockOwner);
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

            var lockedEvents = new List<RelationToOtherEventModel>();

            // Attempt to lock down related, dependent Events down

            // Get dependent events
            var resp = _storage.GetResponses(workflowId, eventId);
            var incl = _storage.GetInclusions(workflowId, eventId);
            var excl = _storage.GetExclusions(workflowId, eventId);

            var allDependentEventsSorted = new SortedDictionary<int, RelationToOtherEventModel>();
            // Set this Event's own lockDto (so the Event know for the future that it locked itself down)
            allDependentEventsSorted.Add(eventId.GetHashCode(), new RelationToOtherEventModel()
            {
                EventId = eventId,
                WorkflowId = workflowId,
                Uri = await _storage.GetUri(workflowId, eventId)
            });

            foreach (var res in resp)
            {
                if(!allDependentEventsSorted.ContainsKey(res.EventId.GetHashCode()))
                {
                    allDependentEventsSorted.Add(res.EventId.GetHashCode(), res);
                }
            }

             foreach (var inc in incl)
            {
                if(!allDependentEventsSorted.ContainsKey(inc.EventId.GetHashCode()))
                {
                    allDependentEventsSorted.Add(inc.EventId.GetHashCode(), inc);
                }
            }

             foreach (var exc in excl)
            {
                if(!allDependentEventsSorted.ContainsKey(exc.EventId.GetHashCode()))
                {
                    allDependentEventsSorted.Add(exc.EventId.GetHashCode(), exc);
                }
            }

        

            // For every related, dependent Event, attempt to lock it
            foreach (var tuple in allDependentEventsSorted)
            {
                var relation = tuple.Value;
                var toLock = new LockDto {LockOwner = eventId, WorkflowId = relation.WorkflowId, Id = relation.EventId};

                try
                {
                    await _eventCommunicator.Lock(relation.Uri, toLock, relation.WorkflowId, relation.EventId);
                    lockedEvents.Add(relation);
                }
                catch (Exception)
                {
                    break;
                }

            }

            if (allDependentEventsSorted.Count != lockedEvents.Count)
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

            var eventsToBeUnlockedSorted = new SortedDictionary<int, RelationToOtherEventModel>();

            foreach (var res in resp)
            {
                if (!eventsToBeUnlockedSorted.ContainsKey(res.EventId.GetHashCode()))
                {
                    eventsToBeUnlockedSorted.Add(res.EventId.GetHashCode(), res);
                }
            }

            foreach (var exc in excl)
            {
                if (!eventsToBeUnlockedSorted.ContainsKey(exc.EventId.GetHashCode()))
                {
                    eventsToBeUnlockedSorted.Add(exc.EventId.GetHashCode(), exc);
                }
            }

            if (eventsToBeUnlockedSorted == null)
            {
                throw new NullReferenceException("eventsToBelocked must not be null");
            }

            // Optimistic approach; assuming every unlocking goes well, everyEventIsUnlocked will go unaffected 
            bool everyEventIsUnlocked = true;

            // Unlock the other Events. 
            foreach (var tuple in eventsToBeUnlockedSorted)
            {
                var relation = tuple.Value;
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
            //unlock self
            await _storage.ClearLock(workflowId, eventId);

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