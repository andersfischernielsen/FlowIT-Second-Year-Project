using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;

namespace Event.Models
{
    /// <summary>
    /// LockLogic is the class that handles the operations regarding locking this Event and other related, dependent Events. 
    /// </summary>
    public class LockLogic
    {
        private readonly EventLogic _logic;
        /// <summary>
        /// Because lock-logic is created for each request at a Controller, this _eventsToBelocked should not become outdated
        /// TODO: Discuss: I am right about above, right?
        /// </summary>
        private readonly IEnumerable<Uri> _eventsToBelocked;
        private HashSet<Uri> _lockedEvents; 
        // TODO: MEGA-ÜBER non-chilled Morten: would an IEventLogic not be preferable / "more ideal" in constructor?
        public LockLogic(EventLogic logic)
        {
            _logic = logic;
            _eventsToBelocked = _logic.GetNotifyDtos().Result; // ER DET HER DEN RIGTIGE LISTE?
            _lockedEvents = new HashSet<Uri>();
        }

        // TODO: Will this method include locking this Event itself? Or should caller take care of this it-self? 
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A task containing a bool value indicating whether the operation of locking all related, dependent
        /// events was successfull or not </returns>
        public async Task<bool> LockAll()
        {
            if (_eventsToBelocked == null)
            {
                throw new InvalidOperationException("_eventsToBelocked is not instantiated");
            }

            // Attempt to lock down related, dependent Events down
            try
            {
                // Initiate the lockDto that is to be passed to the other Events
                // identifing this Event as the lockowner
                LockDto lockDto = new LockDto {LockOwner = _logic.EventId};
                
                // Set this Event's own lockDto (so the Event know for the future that it locked itself down)
                _logic.LockDto = lockDto;

                // For every related, dependent Event, attempt to lock it
                foreach (var uri in _eventsToBelocked)
                {
                    await new EventCommunicator(uri).Lock(lockDto);
                    // Discuss: Aren't we too naive here, just assuming that the Event *actually* DID lock itself? 
                    // What if it failed to do so? We shouldn't add it to the _lockedEvents list then...
                    // TODO: Read above! 
                    _lockedEvents.Add(uri);
                }

                //TODO: Brug Parrallel i stedet for:
                // Parallel.ForEach(_eventsToBelocked, async uri =>
                // {
                //     await new EventCommunicator(uri).Lock(lockDto);
                //     _lockedEvents.Add(uri);
                // });

                return true;
            }
            catch (Exception)
            {
                UnlockSome();
                return false;
            }
        }

        private void UnlockSome()
        {
            EventAddressDto eventAddress = new EventAddressDto() {Id = _logic.EventId, Uri = _logic.OwnUri};

            var parallelTasks = Parallel.ForEach(_lockedEvents, pair =>
            {
                try
                {
                    new EventCommunicator(pair).Unlock(eventAddress).Wait();
                }
                catch (Exception)
                {
                    // TODO: Find out what to do if you cant even unlock. Even.
                }
                
            });

            while (!parallelTasks.IsCompleted)
            {
            }
            _lockedEvents = null;
            _logic.LockDto = null;
        }

        public async Task<bool> UnlockAll()
        {

            if (_eventsToBelocked == null)
            {
                throw new InvalidOperationException("_eventsToBelocked must not be null");
            }

            // Optimistic approach; assuming every unlocking goes well, everyEventIsUnlocked will go unaffected 
            bool everyEventIsUnlocked = true;

            // Create the same identifier, that was used when locking the Events
            var eventAddress = new EventAddressDto() { Id = _logic.EventId, Uri = _logic.OwnUri };

            // Unlock the other Events. 
            // TODO: Do sazzy Parallel.ForEach here, too...?
            foreach (var uri in _eventsToBelocked)
            {
                try
                {
                    await new EventCommunicator(uri).Unlock(eventAddress);
                }
                catch (Exception)
                {
                    // TODO: Find out what to do if you cant even unlock. Even.
                    everyEventIsUnlocked = false;
                }
            }

            // Finally, unlock this Event itself. 
            _logic.UnlockEvent();

            return everyEventIsUnlocked;
        }
    }
}