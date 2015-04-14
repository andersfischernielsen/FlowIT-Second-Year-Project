using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;
using Event.Models;

namespace Event.Storage
{
    public class EventLogic : IEventLogic
    {
        #region Properties
        public Uri OwnUri
        {
            set { Storage.OwnUri = value; }
            get { return Storage.OwnUri; }
        }

        public string WorkflowId
        {
            set { Storage.WorkflowId = value; }
            get { return Storage.WorkflowId; }
        }

        public string EventId
        {
            set { Storage.EventId = value; }
            get { return Storage.EventId; }
        }

        public string Name
        {
            set { Storage.Name = value; }
            get { return Storage.Name; }
        }

        public LockDto LockDto
        {
            set { Storage.LockDto = value; }
            get { return Storage.LockDto; }
        }

        public bool Executed
        {
            set { Storage.Executed = value; }
            get { return Storage.Executed; }
        }

        public bool Included
        {
            set { Storage.Included = value; }
            get { return Storage.Included; }
        }

        public bool Pending
        {
            set { Storage.Pending = value; }
            get { return Storage.Pending; }
        }

        public HashSet<RelationToOtherEventModel> Conditions
        {
            set { Storage.Conditions = value; }
            get { return Storage.Conditions; }
        }

        public HashSet<RelationToOtherEventModel> Responses
        {
            set { Storage.Responses = value; }
            get { return Storage.Responses; }
        }

        public HashSet<RelationToOtherEventModel> Exclusions
        {
            set { Storage.Exclusions = value; }
            get { return Storage.Exclusions; }
        }

        public HashSet<RelationToOtherEventModel> Inclusions
        {
            set { Storage.Inclusions = value; }
            get { return Storage.Inclusions; }
        }

        public IEnumerable<RelationToOtherEventModel> RelationsToLock
        {
            get
            {
                var resp = new HashSet<RelationToOtherEventModel>(Responses);
                var incl = new HashSet<RelationToOtherEventModel>(Inclusions);
                var excl = new HashSet<RelationToOtherEventModel>(Exclusions);
                return new List<RelationToOtherEventModel>(resp.Concat(incl.Concat(excl)));
            }
        }

        // The list of rules a given user has to have for execution.
        public IEnumerable<string> Roles
        {
            get { return Storage.Roles; }
            set { Storage.Roles = value; }
        }

        #endregion

        #region Init
        //Storage instance for getting and setting data.
        private readonly IEventStorage Storage;

        /// <summary>
        /// Constructor for EventLogic
        /// </summary>
        /// <param name="eventId">The id of the event, that this EventLogic instance should represent</param>
        public EventLogic()
        {
            Storage = new EventStorage(new EventContext());
        }
        public EventLogic(IEventStorage storage)
        {
            Storage = storage;
        }

        #endregion

        #region Rule Handling

        public void UnlockEvent()
        {
            Storage.ClearLock();
        }

        public async Task<bool> IsExecutable()
        {
            //If this event is excluded, return false.
            if (!Included)
            {
                return false;
            }

            foreach (var condition in Conditions)
            {
                IEventFromEvent eventCommunicator = new EventCommunicator(condition.Uri, condition.EventID, EventId);
                var executed = await eventCommunicator.IsExecuted();
                var included = await eventCommunicator.IsIncluded();
                // If the condition-event is not executed and currently included.
                if (included && !executed)
                {
                    return false;
                }
            }
            return true; // If all conditions are executed or excluded.
        }

        private static void UpdateRule(bool shouldAdd, RelationToOtherEventModel value, ISet<RelationToOtherEventModel> collection)
        {
            //Todo : Persist this stuff.
            if (shouldAdd) collection.Add(value);
            else collection.Remove(value);
        }

        #endregion

        #region DTO Creation

        public async Task<EventStateDto> GetEventStateDto()
        {

            return new EventStateDto
            {
                Id = EventId,
                Name = Name,
                Executed = Executed,
                Included = Included,
                Pending = Pending,
                Executable = await IsExecutable()
            };
        }

        public EventDto GetEventDto()
        {
                return new EventDto
                {
                    EventId = EventId,
                    WorkflowId = WorkflowId,
                    Name = Name,
                    Roles = Roles,
                    Pending = Pending,
                    Executed = Executed,
                    Included = Included,
                    Conditions = Conditions.Select(model => new EventAddressDto{Id = model.EventID,Uri = model.Uri}),
                    Exclusions = Exclusions.Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri }),
                    Responses = Responses.Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri }),
                    Inclusions = Inclusions.Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri })
                };
        }
        #endregion


        public bool CallerIsAllowedToOperate(string lockOwnerId)
        {
            if (LockDto == null) return true;
            // TODO: Consider implementing an Equals() method on EventAddressDto
            return LockDto.LockOwner.Equals(lockOwnerId);
        }

        public async Task DeleteEvent()
        {
            // Check if Event exists here
            if (!EventIdExists())
            {
                return;
            }

            // Attempt to delete Event from Server
            IServerFromEvent serverCommunicator = new ServerCommunicator("http://flowit.azurewebsites.net/", EventId, WorkflowId);
            await serverCommunicator.DeleteEventFromServer();

            // Delete Event from own Storage
            Storage.DeleteEvent();
        }

        public async Task Execute()
        {
            Executed = true;
            Pending = false;
            var addressDto = new EventAddressDto {Id = EventId, Uri = OwnUri};
            await Task.Run(async () =>
            {
                foreach (var pending in Responses)
                {
                    await new EventCommunicator(pending.Uri,pending.EventID,EventId).SendPending(addressDto);
                }
                foreach (var inclusion in Inclusions)
                {
                    await new EventCommunicator(inclusion.Uri, inclusion.EventID, EventId).SendIncluded(addressDto);
                }
                foreach (var exclusion in Exclusions)
                {
                    await new EventCommunicator(exclusion.Uri, exclusion.EventID, EventId).SendExcluded(addressDto);
                }
            });
        }

        /// <summary>
        /// Checks whether an Event exists in the Storage
        /// </summary>
        /// <returns>Bool value whether or not the Event exists</returns>
        public bool EventIdExists()
        {
            try
            {
                return Storage.Name != null;
            }
            catch (ApplicationException)
            {
                return false;
            }
        }


        /// <summary>
        /// This method determines whether the Event is locked or not. If EventLogic is associated with a non-existing
        /// Event(id), IsLocked will return false still (and not raise an Exception)
        /// </summary>
        /// <returns></returns>
        public bool IsLocked()
        {
            if (!EventIdExists())
            {
                // Hence, 
                return false;
            }

            return LockDto != null;
        }

        public void Dispose()
        {
            Storage.Dispose();
        }

        // TODO: This should be a private method. "Logic" in EventStateController's Update** should be moved inside EventLogic
        public bool ProvidedRolesHasMatchWithEventRoles(IEnumerable<string> providedRoles)
        {
            if (providedRoles.Any(providedRole => Roles.Contains(providedRole)))
            {
                return true;
            }
            return false;
        }
    }
}