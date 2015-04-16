using System;
using System.Net.Http;
using System.Threading.Tasks;
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
        private readonly IAuthLogic _authLogic;

        /// <summary>
        /// Runtime Constructor.
        /// Uses default implementations of IEventStorage, ILockingLogic, IAuthLogic.
        /// </summary>
        public StateLogic()
        {
            _storage = new EventStorage(new EventContext());
            _lockingLogic = null; // Todo: Use actual implementation.
            _authLogic = new AuthLogic(_storage);
        }

        /// <summary>
        /// Constructor used for dependency injection.
        /// </summary>
        /// <param name="storage">An implementation of IEventStorage</param>
        /// <param name="lockingLogic">An implementation of ILockingLogic</param>
        /// <param name="authLogic">An implementation of IAuthLogic</param>
        public StateLogic(IEventStorage storage, ILockingLogic lockingLogic, IAuthLogic authLogic)
        {
            _storage = storage;
            _lockingLogic = lockingLogic;
            _authLogic = authLogic;
        }

        public async Task<bool> IsExecuted(string eventId, string senderId)
        {
            // Check is made to see if caller is allowed to execute this method at the moment. 
            if (!await _lockingLogic.IsAllowedToOperate(eventId, senderId))
            {
                throw new LockedException();
            }
            return await _storage.GetExecuted(eventId);
        }

        public async Task<bool> IsIncluded(string eventId, string senderId)
        {
            // Check is made to see if caller is allowed to execute this method
            if (!await _lockingLogic.IsAllowedToOperate(eventId, senderId))
            {
                throw new LockedException();
            }
            return await _storage.GetIncluded(eventId);
        }

        public async Task<EventStateDto> GetStateDto(string eventId, string senderId)
        {
            //Todo: The client uses this method and sends -1 as an ID. This is a bad solution, so refactoring is encouraged.
            // Check is made to see whether caller is allowed to execute this method at the moment
            if (!senderId.Equals("-1") && !await _lockingLogic.IsAllowedToOperate(eventId, senderId))
            {
                throw new LockedException();
            }

            return new EventStateDto
            {
                Id = eventId,
                Name = await _storage.GetName(eventId),
                Executed = await _storage.GetExecuted(eventId),
                Included = await _storage.GetIncluded(eventId),
                Pending = await _storage.GetPending(eventId),
                Executable = await IsExecutable(eventId)
            };
        }

        private async Task<bool> IsExecutable(string eventId)
        {
            //If this event is excluded, return false.
            if (!await _storage.GetIncluded(eventId))
            {
                return false;
            }

            foreach (var condition in _storage.GetConditions(eventId))
            {
                IEventFromEvent eventCommunicator = new EventCommunicator(condition.Uri, condition.EventID, eventId);
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

        public async Task SetIncluded(string eventId, string senderId, bool newIncludedValue)
        {
            // Check to see if caller is currently allowed to execute this method
            if (!await _lockingLogic.IsAllowedToOperate(eventId, senderId))
            {
                throw new LockedException();
            }
            await _storage.SetIncluded(eventId, newIncludedValue);
        }

        public async Task SetPending(string eventId, string senderId, bool newPendingValue)
        {
            // Check if caller is allowed to execute this method at the moment
            if (!await _lockingLogic.IsAllowedToOperate(eventId, senderId))
            {
                throw new LockedException();
            }
            await _storage.SetPending(eventId, newPendingValue);
        }

        public async Task<bool> Execute(string eventId, ExecuteDto executeDto)
        {
            // Check that caller claims the right role for executing this Event
            if (!await _authLogic.IsAuthorized(eventId, executeDto.Roles))
            {
                throw new NotAuthorizedException();
            }

            // Check if Event is currently locked
            if (!await _lockingLogic.IsAllowedToOperate(eventId, eventId))
            {
                throw new LockedException();
            }
            // Check whether Event can be executed at the moment
            if (!(await IsExecutable(eventId)))
            {
                throw new NotExecutableException();
            }

            // Lock all dependent Events (including one-self)
            // TODO: Check: Does the following include locking on this Event itself...?
            if (!await _lockingLogic.LockAll(eventId))
            {
                throw new FailedToLockOtherEventException();
            }

            var allOk = true;
            Exception exception = null;
            try
            {
                await _storage.SetExecuted(eventId, true);
                await _storage.SetPending(eventId, false);
                var addressDto = new EventAddressDto {Id = eventId, Uri = await _storage.GetUri(eventId)};
                foreach (var pending in _storage.GetResponses(eventId))
                {
                    await new EventCommunicator(pending.Uri, pending.EventID, eventId).SendPending(addressDto);
                }
                foreach (var inclusion in _storage.GetInclusions(eventId))
                {
                    await new EventCommunicator(inclusion.Uri, inclusion.EventID, eventId).SendIncluded(addressDto);
                }
                foreach (var exclusion in _storage.GetExclusions(eventId))
                {
                    await new EventCommunicator(exclusion.Uri, exclusion.EventID, eventId).SendExcluded(addressDto);
                }
            }
            catch (HttpRequestException)
            {
                allOk = false;
                exception = new FailedToUpdateStateAtOtherEventException();
            }
            catch (Exception)
            {
                allOk = false;
                exception = new FailedToUpdateStateException();
            }

            if (!await _lockingLogic.UnlockAll(eventId))
            {
                throw new FailedToUnlockOtherEventException();
                //Kunne ikke unlocke alt, hvad skal der ske?
            }
            if (allOk)
            {
                return true;
            }
            throw exception;
        }

        public void Dispose()
        {
            _storage.Dispose();
            _lockingLogic.Dispose();
            _authLogic.Dispose();
        }
    }
}