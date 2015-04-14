using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Event.Exceptions;
using Event.Interfaces;
using Event.Models;
using Event.Storage;

namespace Event.Logic
{
    public class LockingLogic : ILockingLogic
    {
        private readonly IEventStorage _storage;

        public LockingLogic(IEventStorage storage)
        {
            _storage = storage;
        }

        public void LockSelf(string eventId, LockDto lockDto)
        {
            // Check input
            if (lockDto == null)
            {
                throw new ArgumentNullException("lockDto", "was null");
            }

            // Check if this Event is currently locked
            if (!IsAllowedToOperate(eventId, lockDto.LockOwner))
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
            _storage.SetLockDto(eventId, lockDto);
        }

        public void UnlockSelf(string eventId, string callerId)
        {
            if (!IsAllowedToOperate(eventId, callerId))
            {
                throw new LockedException();
            }
            _storage.ClearLock(eventId);
        }

        public async Task<bool> LockAll(string eventId)
        {
            var eventsToBeLocked = new List<RelationToOtherEventModel>();
            var lockedEvents = new List<RelationToOtherEventModel>();

            // Attempt to lock down related, dependent Events down
            
            // Initiate the lockDto that is to be passed to the other Events
            // identifing this Event as the lockowner
            var lockDto = new LockDto {LockOwner = eventId, Id = eventId};

            // Set this Event's own lockDto (so the Event know for the future that it locked itself down)
            _storage.SetLockDto(eventId, lockDto);


            // Get dependent events
            var resp = _storage.GetResponses(eventId);
            var incl = _storage.GetInclusions(eventId);
            var excl = _storage.GetExclusions(eventId);
            var eventsToBelocked = resp.Concat(incl.Concat(excl));

            // For every related, dependent Event, attempt to lock it
            foreach (var relation in eventsToBelocked)
            {
                var toLock = new LockDto {LockOwner = eventId, Id = relation.EventID};

                try
                {
                    await new EventCommunicator(relation.Uri, relation.EventID, eventId).Lock(toLock);
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
                UnlockSome(eventId,lockedEvents).Wait();

                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns>False if it fails to unlock other Events</returns>
        public async Task<bool> UnlockAll(string eventId)
        {
            // Gather dependent events
            var resp = _storage.GetResponses(eventId);
            var incl = _storage.GetInclusions(eventId);
            var excl = _storage.GetExclusions(eventId);
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
                    await new EventCommunicator(relation.Uri, relation.EventID, eventId).Unlock();
                }
                catch (Exception)
                {
                    // TODO: Find out what to do if you cant even unlock. Even.
                    everyEventIsUnlocked = false;
                }
            }

            return everyEventIsUnlocked;
        }

        public bool IsAllowedToOperate(string eventId, string callerId)
        {
            var lockDto = _storage.GetLockDto(eventId);
            if (lockDto == null)
            {   // No lock is set!
                return true;
            }

            return lockDto.LockOwner.Equals(callerId);
        }

        public void Dispose()
        {
            _storage.Dispose();
        }

        private async Task UnlockSome(string eventId, List<RelationToOtherEventModel> eventsToBeUnlocked)
        {
            // Unlock the other Events. 
            foreach (var relation in eventsToBeUnlocked)
            {
                try
                {
                    await new EventCommunicator(relation.Uri, relation.EventID, eventId).Unlock();
                }
                catch (Exception)
                {
                    // TODO: Find out what to do if you cant even unlock. Even.
                }
            }
        }
    }
}