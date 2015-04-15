using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Event.Exceptions;
using Event.Interfaces;
using Event.Models;
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
            _lockLogic = new LockingLogic(_storage);
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
            if (await EventIdExists(eventDto.EventId))
            {
                // TODO: Throw more relevant exception
                throw new ApplicationException("An event with the Id already exists");
            }

            // #1. Make sure that server will accept our entry
            var dto = new EventAddressDto
            {
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
            var otherEvents = await serverCommunicator.PostEventToServer(dto);

            try
            {
                // Setup a new Event in own database.
                var initialEventState = new InitialEventState()
                {
                    EventId = eventDto.EventId,
                    Executed = eventDto.Executed,
                    Included = eventDto.Included,
                    Pending = eventDto.Pending
                };

                await _storage.InitializeNewEvent(initialEventState);
            }
            catch (Exception)
            {
                // if something goes wrong, we have to delete the event from the server again.
                serverCommunicator.DeleteEventFromServer().Wait();
                throw;
            }
        }

        public async Task DeleteEvent(string eventId)
        {
            // Notice that the following check will (should) fail, if this Event is locked by another Event
            if (! await _lockLogic.IsAllowedToOperate(eventId, eventId))
            {
                throw new LockedException();
            }

            // Check if Event exists here
            if (!await EventIdExists(eventId))
            {
                // No need to do more, event already does not exist
                return;
            }

            // Attempt to delete Event from Server
            string workflowId = await _storage.GetWorkflowId(eventId);

            // Todo: Check that this works correcly when deployed!
#if DEBUG
            IServerFromEvent serverCommunicator = new ServerCommunicator("http://localhost:13768/", eventId, workflowId);
#else
            IServerFromEvent serverCommunicator = new ServerCommunicator("http://flowit.azurewebsites.net/", eventId, workflowId);
#endif
            await serverCommunicator.DeleteEventFromServer();

            // Delete Event from own Storage
            await _storage.DeleteEvent(eventId);
        }

        /// <summary>
        /// ResetEvent will bruteforce reset this Event, regardless of whether it is currently locked
        /// </summary>
        /// <param name="eventId"></param>
        public async Task ResetEvent(string eventId)
        {
            // Clear lock
            await _resetStorage.ClearLock(eventId);

            // Reset to initial state
            await _resetStorage.ResetToInitialState(eventId);
        }

        public async Task<EventDto> GetEventDto(string eventId)
        {
            var returnValue =  new EventDto
            {
                EventId = eventId,
                WorkflowId = await _storage.GetWorkflowId(eventId),
                Name = await _storage.GetName(eventId),
                Roles = await _storage.GetRoles(eventId),
                Pending = await _storage.GetPending(eventId),
                Executed = await _storage.GetExecuted(eventId),
                Included = await _storage.GetIncluded(eventId),
                Conditions = _storage.GetConditions(eventId).Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri }),
                Exclusions = _storage.GetExclusions(eventId).Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri }),
                Responses = _storage.GetResponses(eventId).Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri }),
                Inclusions = _storage.GetInclusions(eventId).Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri })
            };
            return returnValue;
        }

        public void Dispose()
        {
            _storage.Dispose();
        }

        private async Task<bool> EventIdExists(string eventId)
        {
            try
            {
                return await _storage.GetName(eventId) != null;
            }
            catch (ApplicationException)
            {
                return false;
            }
        }
    }
}