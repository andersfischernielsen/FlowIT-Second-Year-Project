using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Exceptions;
using Event.Communicators;
using Event.Exceptions;
using Event.Interfaces;
using Event.Models;
using Event.Models.UriClasses;
using Event.Storage;

namespace Event.Logic
{
    public class LifecycleLogic : ILifecycleLogic
    {
        private readonly IEventStorage _storage;
        private readonly IEventStorageForReset _resetStorage;
        private readonly ILockingLogic _lockLogic;

        // Default constructor
        public LifecycleLogic()
        {
            var context = new EventContext();
            _storage = new EventStorage(context);
            _resetStorage = new EventStorageForReset(context);
            _lockLogic = new LockingLogic(_storage, new EventCommunicator());
        }

        // Constructor to be used for dependency-injection
        public LifecycleLogic(IEventStorage storage, IEventStorageForReset resetStorage, ILockingLogic lockLogic)
        {
            _storage = storage;
            _resetStorage = resetStorage;
            _lockLogic = lockLogic;
        }


        public async Task CreateEvent(EventDto eventDto, Uri ownUri)
        {
            if (eventDto == null)
            {
                throw new ArgumentNullException("eventDto", "Provided EventDto was null");
            }
            if (ownUri == null)
            {
                throw new ArgumentNullException("ownUri", "Provided Uri was null");   
            }
            if (await _storage.Exists(eventDto.WorkflowId, eventDto.EventId))
            {
                // TODO: Throw more relevant exception
                throw new ApplicationException("An event with the Id already exists");
            }

            // #1. Make sure that server will accept our entry
            var dto = new EventAddressDto
            {
                WorkflowId = eventDto.WorkflowId,
                Id = eventDto.EventId,
                Uri = ownUri,
                Roles = eventDto.Roles
            };

            // Todo: Check that this works correcly when deployed!
#if DEBUG
            IServerFromEvent serverCommunicator = new ServerCommunicator("http://localhost:13768/", eventDto.EventId, eventDto.WorkflowId);
#else
            IServerFromEvent serverCommunicator = new ServerCommunicator("http://flowit.azurewebsites.net/", eventDto.EventId, eventDto.WorkflowId);
#endif
            // TODO: try-catch here?
            // Todo: Do we need what this method returns or is it waste of data-transfer?
            await serverCommunicator.PostEventToServer(dto);
            try
            {
                // Setup a new Event in own database.
                var @event = new EventModel
                {
                    Id = eventDto.EventId,
                    WorkflowId = eventDto.WorkflowId,
                    Name = eventDto.Name,
                    Roles = eventDto.Roles.Select(role => new EventRoleModel {WorkflowId = eventDto.WorkflowId, EventId = eventDto.EventId, Role = role}).ToList(),
                    OwnUri = ownUri.AbsoluteUri,
                    Executed = eventDto.Executed,
                    Included = eventDto.Included,
                    Pending = eventDto.Pending,
                    ConditionUris = eventDto.Conditions.Select(condition => new ConditionUri{EventId = condition.Id, UriString = condition.Uri.AbsoluteUri}).ToList(),
                    ResponseUris = eventDto.Responses.Select(response => new ResponseUri{EventId = response.Id, UriString = response.Uri.AbsoluteUri}).ToList(),
                    InclusionUris = eventDto.Responses.Select(inclusion => new InclusionUri{EventId = inclusion.Id, UriString = inclusion.Uri.AbsoluteUri}).ToList(),
                    ExclusionUris = eventDto.Responses.Select(exclusion => new ExclusionUri{EventId = exclusion.Id, UriString = exclusion.Uri.AbsoluteUri}).ToList(),
                    LockDto = null,
                    InitialExecuted = eventDto.Executed,
                    InitialIncluded = eventDto.Included,
                    InitialPending = eventDto.Pending
                };

                await _storage.InitializeNewEvent(@event);
            }
            catch (Exception)
            {
                // if something goes wrong, we have to delete the event from the server again.
                serverCommunicator.DeleteEventFromServer().Wait();
                throw;
            }
        }

        public async Task DeleteEvent(string workflowId, string eventId)
        {
            // Notice that the following check will (should) fail, if this Event is locked by another Event
            if (! await _lockLogic.IsAllowedToOperate(workflowId, eventId, eventId))
            {
                throw new LockedException();
            }

            // Check if Event exists here
            if (!await _storage.Exists(workflowId, eventId))
            {
                // No need to do more, event already does not exist
                return;
            }

            // Todo: Check that this works correcly when deployed!
#if DEBUG
            IServerFromEvent serverCommunicator = new ServerCommunicator("http://localhost:13768/", eventId, workflowId);
#else
            IServerFromEvent serverCommunicator = new ServerCommunicator("http://flowit.azurewebsites.net/", eventId, workflowId);
#endif
            await serverCommunicator.DeleteEventFromServer();

            // Delete Event from own Storage
            await _storage.DeleteEvent(workflowId, eventId);
        }

        /// <summary>
        /// ResetEvent will bruteforce reset this Event, regardless of whether it is currently locked
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventId"></param>
        public async Task ResetEvent(string workflowId, string eventId)
        {
            // Clear lock
            await _resetStorage.ClearLock(workflowId, eventId);

            // Reset to initial state
            await _resetStorage.ResetToInitialState(workflowId, eventId);
        }

        public async Task<EventDto> GetEventDto(string workflowId, string eventId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException("workflowId");
            }
            if (eventId == null)
            {
                throw new ArgumentNullException("eventId","was null");
            }

            if (!await _storage.Exists(workflowId, eventId))
            {
                return null;
            }

            var returnValue =  new EventDto
            {
                EventId = eventId,
                WorkflowId = workflowId,
                Name = await _storage.GetName(workflowId, eventId),
                Roles = await _storage.GetRoles(workflowId, eventId),
                Pending = await _storage.GetPending(workflowId, eventId),
                Executed = await _storage.GetExecuted(workflowId, eventId),
                Included = await _storage.GetIncluded(workflowId, eventId),
                Conditions = _storage.GetConditions(workflowId, eventId).Select(model => new EventAddressDto { WorkflowId = model.WorkflowId, Id = model.EventId, Uri = model.Uri }),
                Exclusions = _storage.GetExclusions(workflowId, eventId).Select(model => new EventAddressDto { WorkflowId = model.WorkflowId, Id = model.EventId, Uri = model.Uri }),
                Responses = _storage.GetResponses(workflowId, eventId).Select(model => new EventAddressDto { WorkflowId = model.WorkflowId, Id = model.EventId, Uri = model.Uri }),
                Inclusions = _storage.GetInclusions(workflowId, eventId).Select(model => new EventAddressDto { WorkflowId = model.WorkflowId, Id = model.EventId, Uri = model.Uri })
            };
            return returnValue;
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}