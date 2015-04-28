using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Common.Exceptions;
using Event.Interfaces;
using Event.Models;

namespace Event.Logic
{
    /// <summary>
    /// LockingLogic handles the logic involved in operations on Event locks. 
    /// </summary>
    public class LockingLogic : ILockingLogic
    {
        private readonly IEventStorage _storage;
        private readonly IEventFromEvent _eventCommunicator;

        //QUEUE is holding a dictionary of string (workflowid) , dictionary which holds string (eventid), the queue
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentQueue<LockDto>>> _lockQueue = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentQueue<LockDto>>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage">Implementation of IEventStorage</param>
        /// <param name="eventCommunicator">Instance of IEventFromEvent, that will be used for communication
        /// to another Event.</param>
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
        /// <exception cref="ArgumentNullException">Thrown if any of the provided arguments are null</exception>
        /// <exception cref="ArgumentException">Thrown if the arguments are non-sensible</exception>


        public static void AddToQueue(string workflowId, string eventId, LockDto lockDto)
        {
            var eventDictionary = _lockQueue.GetOrAdd(workflowId, new ConcurrentDictionary<string, ConcurrentQueue<LockDto>>());
            var queue = eventDictionary.GetOrAdd(eventId, new ConcurrentQueue<LockDto>());
            queue.Enqueue(lockDto);
        }

        public static LockDto Dequeue(string workflowId, string eventId)
        {
            var eventDictionary = _lockQueue.GetOrAdd(workflowId, new ConcurrentDictionary<string, ConcurrentQueue<LockDto>>());
            var queue = eventDictionary.GetOrAdd(eventId, new ConcurrentQueue<LockDto>());
            LockDto next;
            queue.TryDequeue(out next);
            return next;
        }

        public static bool AmINext(string workflowId, string eventId, LockDto lockDto)
        {
            var eventDictionary = _lockQueue.GetOrAdd(workflowId, new ConcurrentDictionary<string, ConcurrentQueue<LockDto>>());
            var queue = eventDictionary.GetOrAdd(eventId, new ConcurrentQueue<LockDto>());
            LockDto next;
            if (queue.TryPeek(out next))
            {
                return next.LockOwner == lockDto.LockOwner && next.WorkflowId == lockDto.WorkflowId;
            }
            return false;
        }

        public async Task WaitForMyTurn(string workflowId, string eventId, LockDto lockDto)
        {
            // Check input
            if (workflowId == null || lockDto == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            // Checks that the provided lockDto actually has sensible values for its fields.
            if (String.IsNullOrEmpty(lockDto.LockOwner) || String.IsNullOrWhiteSpace(lockDto.LockOwner))
            {
                // Reject request on setting the lockDto
                throw new ArgumentException("lockDto.lockOwner was null");
            }

            //Add to queue
            AddToQueue(workflowId, eventId, lockDto);

            // Check if this Event is currently locked
            if (!await IsAllowedToOperate(workflowId, eventId, lockDto.LockOwner))
            {
                var watch = new Stopwatch();
                watch.Start();
                //todo: ugly mayby?
                while (!AmINext(workflowId, eventId, lockDto))
                {
                    if (watch.Elapsed.Seconds > 10)
                    {
                        //Waited too long in queue
                        throw new LockedException();
                    }
                    await Task.Delay(100);
                }
                await _storage.Reload(workflowId, eventId);
            }
            Dequeue(workflowId, eventId);
        }

        public async Task LockSelf(string workflowId, string eventId, LockDto lockDto)
        {
            // Check input
            if (workflowId == null || lockDto == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            // Checks that the provided lockDto actually has sensible values for its fields.
            if (String.IsNullOrEmpty(lockDto.LockOwner) || String.IsNullOrWhiteSpace(lockDto.LockOwner))
            {
                // Reject request on setting the lockDto
                throw new ArgumentException("lockDto.lockOwner was null");
            }


            //Add to queue
            AddToQueue(workflowId, eventId, lockDto);

            // Check if this Event is currently locked
            if (!await IsAllowedToOperate(workflowId, eventId, lockDto.LockOwner))
            {
                var watch = new Stopwatch();
                watch.Start();
                //todo: ugly mayby?
                while (!AmINext(workflowId, eventId, lockDto))
                {
                    if (watch.Elapsed.Seconds > 10)
                    {
                        //Waited too long in queue
                        throw new LockedException();
                    }
                    await Task.Delay(100);
                }
                await _storage.Reload(workflowId, eventId);
            }

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

            if (eventId == null)
            {
                throw new ArgumentNullException("callerId", "callerId was null");
            }
            if (callerId == null)
            {
                throw new ArgumentNullException("eventId", "eventId was null");
            }

            if (!await IsAllowedToOperate(workflowId, eventId, callerId))
            {
                throw new LockedException();
            }

            await _storage.ClearLock(workflowId, eventId);
            Dequeue(workflowId, eventId);
        }

        /// <summary>
        /// LockAllForExecute attempts to lockall related Events for the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<bool> LockAllForExecute(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            // Get dependent events
            var resp = await _storage.GetResponses(workflowId, eventId);
            var incl = await _storage.GetInclusions(workflowId, eventId);
            var excl = await _storage.GetExclusions(workflowId, eventId);            

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
                if (!allDependentEventsSorted.ContainsKey(res.EventId.GetHashCode()))
                {
                    allDependentEventsSorted.Add(res.EventId.GetHashCode(), res);
                }
            }

            foreach (var inc in incl)
            {
                if (!allDependentEventsSorted.ContainsKey(inc.EventId.GetHashCode()))
                {
                    allDependentEventsSorted.Add(inc.EventId.GetHashCode(), inc);
                }
            }

            foreach (var exc in excl)
            {
                if (!allDependentEventsSorted.ContainsKey(exc.EventId.GetHashCode()))
                {
                    allDependentEventsSorted.Add(exc.EventId.GetHashCode(), exc);
                }
            }

            return await LockList(allDependentEventsSorted, eventId);

        }


        public async Task<bool> LockList(SortedDictionary<int, RelationToOtherEventModel> list, string eventId)
        {
            var lockedEvents = new List<RelationToOtherEventModel>();
            // For every related, dependent Event, attempt to lock it
            foreach (var tuple in list)
            {
                var relation = tuple.Value;
                var toLock = new LockDto { LockOwner = eventId, WorkflowId = relation.WorkflowId, Id = relation.EventId };

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

            if (list.Count != lockedEvents.Count)
            {
                // TODO: May be an error here, if one list contains this Event itself, while the other does not. 
                await UnlockSome(eventId, lockedEvents);

                return false;
            }
            return true;
        }

        /// <summary>
        /// UnlockAllForExecute attempts to unlock all related events for the specified Event. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns>False if it fails to unlock other Events</returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NullReferenceException">Thrown if Storage layer returns null-relations.</exception>
        public async Task<bool> UnlockAllForExecute(string workflowId, string eventId)
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

            var eventsToBeUnlockedSorted = new SortedDictionary<int, RelationToOtherEventModel>();

            foreach (var res in resp)
            {
                if (!eventsToBeUnlockedSorted.ContainsKey(res.EventId.GetHashCode()))
                {
                    eventsToBeUnlockedSorted.Add(res.EventId.GetHashCode(), res);
                }
            }

            foreach (var inc in incl)
            {
                if (!eventsToBeUnlockedSorted.ContainsKey(inc.EventId.GetHashCode()))
                {
                    eventsToBeUnlockedSorted.Add(inc.EventId.GetHashCode(), inc);
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

            var b = await UnlockList(eventsToBeUnlockedSorted, eventId);
            
            
            await _storage.ClearLock(workflowId, eventId);
            
            return b;
        }

        public async Task<bool> UnlockList(SortedDictionary<int, RelationToOtherEventModel> list, string eventId)
        {
            var everyEventIsUnlocked = true;
            foreach (var tuple in list)
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
        private async Task UnlockSome(string eventId, List<RelationToOtherEventModel> eventsToBeUnlocked)
        {
            if (eventId == null || eventsToBeUnlocked == null)
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