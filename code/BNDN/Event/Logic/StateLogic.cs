using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Common;
using Event.Exceptions;
using Event.Interfaces;
using Event.Storage;

namespace Event.Logic
{
    public class StateLogic : IStateLogic
    {
        private readonly IEventStorage _storage;
        private readonly ILockingLogic _lockingLogic;

        public StateLogic()
        {
            _storage = new EventStorage(new EventContext());
            _lockingLogic = null; // Todo: Use actual implementation.
        }

        public StateLogic(IEventStorage storage, ILockingLogic lockingLogic)
        {
            _storage = storage;
            _lockingLogic = lockingLogic;
        }

        public bool IsExecuted(string eventId, string senderId)
        {
            // Check is made to see if caller is allowed to execute this method at the moment. 
            if (!_lockingLogic.IsAllowedToOperate(eventId, senderId))
            {
                throw new LockedException();
            }
            return _storage.Executed;
        }

        public bool IsIncluded(string eventId, string senderId)
        {
            // Check is made to see if caller is allowed to execute this method
            if (!_lockingLogic.IsAllowedToOperate(eventId, senderId))
            {
                throw new LockedException();
            }
            return _storage.Included;
        }

        public async Task<EventStateDto> GetStateDto(string eventId, string senderId)
        {
            //Todo: The client uses this method and sends -1 as an ID. This is a bad solution, so refactoring is encouraged.
            // Check is made to see whether caller is allowed to execute this method at the moment
            if (!senderId.Equals("-1") && !_lockingLogic.IsAllowedToOperate(eventId, senderId))
            {
                throw new LockedException();
            }

            return new EventStateDto
            {
                Id = _storage.EventId,
                Name = _storage.Name,
                Executed = _storage.Executed,
                Included = _storage.Included,
                Pending = _storage.Pending,
                Executable = await IsExecutable(eventId)
            };
        }

        private async Task<bool> IsExecutable(string eventId)
        {
            //If this event is excluded, return false.
            if (!_storage.Included)
            {
                return false;
            }

            foreach (var condition in _storage.Conditions)
            {
                using (IEventFromEvent eventCommunicator = new EventCommunicator(condition.Uri, condition.EventID, eventId)) {
                var executed = await eventCommunicator.IsExecuted();
                var included = await eventCommunicator.IsIncluded();
                // If the condition-event is not executed and currently included.
                if (included && !executed)
                {
                    return false;
                }
}
            }
            return true; // If all conditions are executed or excluded.
        }

        public void SetIncluded(string eventId, bool newIncludedValue)
        {
            throw new NotImplementedException();
        }

        public void SetPending(string eventId, bool newPendingValue)
        {
            throw new NotImplementedException();
        }

        public void Execute(string eventId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}