using System;
using System.Threading.Tasks;
using Common;
using Common.Exceptions;
using Event.Communicators;
using Event.Exceptions;
using Event.Interfaces;
using Event.Models;
using Event.Storage;

namespace Event.Logic
{
    /// <summary>
    /// StateLogic is a logic-layer, that handles logic involved in operations on an Event's state. 
    /// </summary>
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
            _storage = new EventStorage();
            _eventCommunicator = new EventCommunicator();
            _lockingLogic = new LockingLogic(_storage, _eventCommunicator);
            _authLogic = new AuthLogic(_storage);
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
            if (workflowId == null || workflowId == null || senderId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // Check is made to see if caller is allowed to execute this method at the moment. 
            if (!await _lockingLogic.IsAllowedToOperate(workflowId, eventId, senderId))
            {
                await _lockingLogic.WaitForMyTurn(workflowId, eventId, new LockDto()
                {
                    WorkflowId = workflowId,
                    LockOwner = senderId,
                    Id = eventId
                });
            }

            return await _storage.GetExecuted(workflowId, eventId);
        }

        public async Task<bool> IsIncluded(string workflowId, string eventId, string senderId)
        {
            if (workflowId == null || workflowId == null || senderId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // Check is made to see if caller is allowed to execute this method
            if (!await _lockingLogic.IsAllowedToOperate(workflowId, eventId, senderId))
            {
                await _lockingLogic.WaitForMyTurn(workflowId, eventId, new LockDto()
                {
                    WorkflowId = workflowId,
                    LockOwner = senderId,
                    Id = eventId
                });
            }

            var b = await _storage.GetIncluded(workflowId, eventId);
            return b;
        }

        // TODO: Discuss: Should this not be moved into a method on EventStorage?
        public async Task<EventStateDto> GetStateDto(string workflowId, string eventId, string senderId)
        {
            // Input check
            if (workflowId == null || workflowId == null || senderId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }



            if (!await _lockingLogic.IsAllowedToOperate(workflowId, eventId, senderId))
            {
                await _lockingLogic.WaitForMyTurn(workflowId,eventId,new LockDto()
                {
                    Id = eventId,
                    WorkflowId = workflowId,
                    LockOwner = senderId
                });
            }

            var name = await _storage.GetName(workflowId, eventId);
            var executed = await _storage.GetExecuted(workflowId, eventId);
            var included = await _storage.GetIncluded(workflowId, eventId);
            var pending = await _storage.GetPending(workflowId, eventId);

            var eventStateDto = new EventStateDto
            {
                Id = eventId,
                Name = name,
                Executed = executed,
                Included = included,
                Pending = pending,
                Executable = await IsExecutable(workflowId, eventId)
            };

            return eventStateDto;
        }

        /// <summary>
        /// Determines whether an Event can be executed at the moment. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        private async Task<bool> IsExecutable(string workflowId, string eventId)
        {
            if (workflowId == null || workflowId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await _storage.GetIncluded(workflowId, eventId))
            {
                //If this event is excluded, return false.
                return false;
            }

            var conditionRelations = await _storage.GetConditions(workflowId, eventId);

            foreach (var condition in conditionRelations)
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
            if (workflowId == null || eventId == null || senderId == null)
            {
                throw new ArgumentNullException();
            }

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
            if (workflowId == null || eventId == null || senderId == null)
            {
                throw new ArgumentNullException();
            }

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

        // TODO: Discuss why is Execute() a Task<bool>?
        public async Task<bool> Execute(string workflowId, string eventId, RoleDto executeDto)
        {
            if (workflowId == null || eventId == null || executeDto == null)
            {
                throw new ArgumentNullException();
            }

            if (!await _storage.Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // Check that caller claims the right role for executing this Event
            if (!await _authLogic.IsAuthorized(workflowId, eventId, executeDto.Roles))
            {
                throw new UnauthorizedException();
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
            if (!await _lockingLogic.LockAllForExecute(workflowId, eventId))
            {
                throw new FailedToLockOtherEventException();
            }

            var allOk = true;
            FailedToUpdateStateAtOtherEventException exception = null;
            try
            {
                var addressDto = new EventAddressDto
                {
                    WorkflowId = workflowId,
                    Id = eventId,
                    Uri = await _storage.GetUri(workflowId, eventId)
                };
                foreach (var pending in await _storage.GetResponses(workflowId, eventId))
                {
                    await _eventCommunicator.SendPending(pending.Uri, addressDto, pending.WorkflowId, pending.EventId);
                }
                foreach (var inclusion in await _storage.GetInclusions(workflowId, eventId))
                {
                    await
                        _eventCommunicator.SendIncluded(inclusion.Uri, addressDto, inclusion.WorkflowId,
                            inclusion.EventId);
                }
                foreach (var exclusion in await _storage.GetExclusions(workflowId, eventId))
                {
                    await
                        _eventCommunicator.SendExcluded(exclusion.Uri, addressDto, exclusion.WorkflowId,
                            exclusion.EventId);
                }
                // There might have been made changes on the entity itself in another controller-call
                // Therefore we have to reload the state from database.
                await _storage.Reload(workflowId, eventId);

                await _storage.SetExecuted(workflowId, eventId, true);
                await _storage.SetPending(workflowId, eventId, false);
            }
            catch (Exception)
            {
                /*  This will catch any of FailedToUpdate<Excluded|Pending|Executed>AtAnotherEventExceptions
                 *  plus other unexpected thrown Exceptions */
                allOk = false;
                exception = new FailedToUpdateStateAtOtherEventException();
            }

            if (!await _lockingLogic.UnlockAllForExecute(workflowId, eventId))
            {
                // If we cannot even unlock, we give up!
                throw new FailedToUnlockOtherEventException();      
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