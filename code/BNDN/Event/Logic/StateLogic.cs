using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Event.Communicators;
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
        private readonly IEventFromEvent _eventCommunicator;

        /// <summary>
        /// Runtime Constructor.
        /// Uses default implementations of IEventStorage, ILockingLogic, IAuthLogic and IEventFromEvent.
        /// </summary>
        public StateLogic()
        {
            _storage = new EventStorage(new EventContext());
            _lockingLogic = null; // Todo: Use actual implementation.
            _authLogic = new AuthLogic(_storage);
            _eventCommunicator = new EventCommunicator();
        }

        /// <summary>
        /// Constructor used for dependency injection.
        /// </summary>
        /// <param name="storage">An implementation of IEventStorage</param>
        /// <param name="lockingLogic">An implementation of ILockingLogic</param>
        /// <param name="authLogic">An implementation of IAuthLogic</param>
        /// <param name="eventCommunicator">An implementation of IEventFromEvent</param>
        public StateLogic(IEventStorage storage, ILockingLogic lockingLogic, IAuthLogic authLogic, IEventFromEvent eventCommunicator)
        {
            _storage = storage;
            _lockingLogic = lockingLogic;
            _authLogic = authLogic;
            _eventCommunicator = eventCommunicator;
        }

        public async Task<bool> IsExecuted(string workflowId, string eventId, string senderId)
        {
            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // Check is made to see if caller is allowed to execute this method at the moment. 
            if (!await _lockingLogic.IsAllowedToOperate(workflowId, eventId, senderId))
            {
                throw new LockedException();
            }
            return await _storage.GetExecuted(workflowId, eventId);
        }

        public async Task<bool> IsIncluded(string workflowId, string eventId, string senderId)
        {
            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // Check is made to see if caller is allowed to execute this method
            if (!await _lockingLogic.IsAllowedToOperate(workflowId, eventId, senderId))
            {
                throw new LockedException();
            }
            return await _storage.GetIncluded(workflowId, eventId);
        }

        public async Task<EventStateDto> GetStateDto(string workflowId, string eventId, string senderId)
        {
            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            //Todo: The client uses this method and sends -1 as an ID. This is a bad solution, so refactoring is encouraged.
            // Check is made to see whether caller is allowed to execute this method at the moment
            if (!senderId.Equals("-1") && !await _lockingLogic.IsAllowedToOperate(workflowId, eventId, senderId))
            {
                throw new LockedException();
            }

            return new EventStateDto
            {
                Id = eventId,
                Name = await _storage.GetName(workflowId, eventId),
                Executed = await _storage.GetExecuted(workflowId, eventId),
                Included = await _storage.GetIncluded(workflowId, eventId),
                Pending = await _storage.GetPending(workflowId, eventId),
                Executable = await IsExecutable(workflowId, eventId)
            };
        }

        private async Task<bool> IsExecutable(string workflowId, string eventId)
        {
            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            //If this event is excluded, return false.
            if (!await _storage.GetIncluded(workflowId, eventId))
            {
                return false;
            }

            foreach (var condition in _storage.GetConditions(workflowId, eventId))
            {
                var executed = await _eventCommunicator.IsExecuted(condition.Uri, condition.WorkflowId, condition.EventId, eventId);
                var included = await _eventCommunicator.IsIncluded(condition.Uri, condition.WorkflowId, condition.EventId, eventId);
                // If the condition-event is not executed and currently included.
                if (included && !executed)
                {
                    return false;
                }
            }
            return true; // If all conditions are executed or excluded.
        }

        public async Task SetIncluded(string workflowId, string eventId, string senderId, bool newIncludedValue)
        {
            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // Check to see if caller is currently allowed to execute this method
            if (!await _lockingLogic.IsAllowedToOperate(workflowId, eventId, senderId))
            {
                throw new LockedException();
            }
            await _storage.SetIncluded(workflowId, eventId, newIncludedValue);
        }

        public async Task SetPending(string workflowId, string eventId, string senderId, bool newPendingValue)
        {
            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // Check if caller is allowed to execute this method at the moment
            if (!await _lockingLogic.IsAllowedToOperate(workflowId, eventId, senderId))
            {
                throw new LockedException();
            }
            await _storage.SetPending(workflowId, eventId, newPendingValue);
        }

        public async Task<bool> Execute(string workflowId, string eventId, RoleDto executeDto)
        {
            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // Check that caller claims the right role for executing this Event
            if (!await _authLogic.IsAuthorized(workflowId, eventId, executeDto.Roles))
            {
                throw new NotAuthorizedException();
            }

            // Check if Event is currently locked
            if (!await _lockingLogic.IsAllowedToOperate(workflowId, eventId, eventId))
            {
                throw new LockedException();
            }
            // Check whether Event can be executed at the moment
            if (!(await IsExecutable(workflowId, eventId)))
            {
                throw new NotExecutableException();
            }

            // Lock all dependent Events (including one-self)
            // TODO: Check: Does the following include locking on this Event itself...?
            if (!await _lockingLogic.LockAll(workflowId, eventId))
            {
                throw new FailedToLockOtherEventException();
            }

            var allOk = true;
            Exception exception = null;
            try
            {
                await _storage.SetExecuted(workflowId, eventId, true);
                await _storage.SetPending(workflowId, eventId, false);
                var addressDto = new EventAddressDto {Id = eventId, Uri = await _storage.GetUri(workflowId, eventId)};
                foreach (var pending in _storage.GetResponses(workflowId, eventId))
                {
                    await _eventCommunicator.SendPending(pending.Uri, addressDto, pending.WorkflowId, pending.EventId);
                }
                foreach (var inclusion in _storage.GetInclusions(workflowId, eventId))
                {
                    await _eventCommunicator.SendIncluded(inclusion.Uri, addressDto, inclusion.WorkflowId, inclusion.EventId);
                }
                foreach (var exclusion in _storage.GetExclusions(workflowId, eventId))
                {
                    await _eventCommunicator.SendExcluded(exclusion.Uri, addressDto, exclusion.WorkflowId, exclusion.EventId);
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

            if (!await _lockingLogic.UnlockAll(workflowId, eventId))
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
            _eventCommunicator.Dispose();
        }
    }
}