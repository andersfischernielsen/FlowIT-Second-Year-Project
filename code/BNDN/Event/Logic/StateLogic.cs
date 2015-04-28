using System;
using System.Collections.Generic;
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

        /// <summary>
        /// IsExecuted returns the executed value for the specified Event. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="senderId">Id of the one, who wants this information.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">Thrown if the specified Event does not exist</exception>
        /// <exception cref="LockedException">Thrown if the Event is locked by someone else than caller</exception>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
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
                throw new LockedException();
            }

            return await _storage.GetExecuted(workflowId, eventId);
        }


        /// <summary>
        /// IsIncluded returns the included value for the specified Event. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="senderId">Id of the one, who wants this information.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">Thrown if the specified Event does not exist</exception>
        /// <exception cref="LockedException">Thrown if the Event is locked by someone else than caller</exception>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
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
                throw new LockedException();
            }

            return await _storage.GetIncluded(workflowId, eventId);   
        }

        // TODO: Discuss: Should this not be moved into a method on EventStorage?
        /// <summary>
        /// GetStateDto returns an EventStateDto for the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="senderId">Id of the one, who wants this information.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if the specified Event does not exist</exception>
        /// <exception cref="LockedException">Thrown if the Event is currently locked</exception>
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


            var conditionRelations = await _storage.GetConditions(workflowId, eventId);
            var lockOrder = new SortedDictionary<int, RelationToOtherEventModel>();

            //adds us self
            lockOrder.Add(eventId.GetHashCode(), new RelationToOtherEventModel()
            {
                EventId = eventId,
                WorkflowId = workflowId,
                Uri = await _storage.GetUri(workflowId, eventId)
            });

            foreach (var con in conditionRelations)
            {
                lockOrder.Add(con.EventId.GetHashCode(), con);
            }

            var b = await _lockingLogic.LockList(lockOrder, eventId);

            if (!b)
            {
                throw new LockedException();
            }

            var eventStateDto = new EventStateDto
            {
                Id = eventId,
                Name = await _storage.GetName(workflowId, eventId),
                Executed = await _storage.GetExecuted(workflowId, eventId),
                Included = await _storage.GetIncluded(workflowId, eventId),
                Pending = await _storage.GetPending(workflowId, eventId),
                Executable = await IsExecutable(workflowId, eventId)
            };

            await _lockingLogic.UnlockList(lockOrder, eventId);
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

        /// <summary>
        /// SetIncluded sets the specified Event's Included value to the provided value. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="senderId">Id of the one, who wants this information.</param>
        /// <param name="newIncludedValue">The value that the Event's Included value should be set to</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the string-type arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if the specified Event does not exist</exception>
        /// <exception cref="LockedException">Thrown if the specified Event is currently locked</exception>
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

        /// <summary>
        /// SetPending sets the specified Event's Pending value to the provided value. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="senderId">Id of the one, who wants this information.</param>
        /// <param name="newPendingValue">The value that the Event's Included value should be set to</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the string-type arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if the specified Event does not exist</exception>
        /// <exception cref="LockedException">Thrown if the specified Event is currently locked</exception>
        /// <returns></returns>
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
        /// <summary>
        /// Execute attempts to Execute the specified Event. The process includes locking the other events, and updating their state. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="executeDto">Contains the roles, that caller has.</param>
        /// <returns></returns>
        /// <exception cref="LockedException">Thrown if the specified Event is currently locked by someone else</exception>
        /// <exception cref="ArgumentNullException">Thrown if any of the provided arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if the specified Event does not exist</exception>
        /// <exception cref="FailedToLockOtherEventException">Thrown if locking of an other (dependent) Event failed.</exception>
        /// <exception cref="FailedToUpdateStateAtOtherEventException">Thrown if updating of another Event's state failed</exception>
        /// <exception cref="FailedToUnlockOtherEventException">Thrown if unlocking of another Event fails.</exception>
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
            // TODO: Check: Does the following include locking on this Event itself...?
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