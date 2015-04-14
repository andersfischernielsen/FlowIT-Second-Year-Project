using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Common;
using Event.Interfaces;
using Event.Models;
using Event.Storage;

namespace Event
{
    public class LifecycleLogic : ILifecycleLogic
    {
        private readonly IEventStorage _storage;

        public LifecycleLogic(IEventStorage storage)
        {
            _storage = storage;
        }


        public Task CreateEvent(EventDto eventDto, Uri ownUri)
        {
            // Set eventId 
            _storage.EventId = eventDto.EventId;

            if (EventIdExists())
            {
                // TODO: Throw more relevant exception
                throw new ApplicationException("An event with the Id already exists");
            }

            if (eventDto == null)
            {
                throw new ArgumentNullException("eventDto", "Provided EventDto was null");
            }
            if (!eventDto.EventId.Equals(_storage.EventId))
            {
                throw new ArgumentException("EventIds does not match!", "eventDto");
            }

            // #1. Make sure that server will accept our entry
            var dto = new EventAddressDto
            {
                Id = eventDto.EventId,
                Uri = ownUri,
                Roles = eventDto.Roles
            };

            IServerFromEvent serverCommunicator = new ServerCommunicator("http://flowit.azurewebsites.net/", eventDto.EventId, eventDto.WorkflowId);
            var otherEvents = await serverCommunicator.PostEventToServer(dto);

            try
            {
                // Setup a new Event in database.
                var initialEventState = new InitialEventState()
                {
                    EventId = eventDto.EventId,
                    Executed = eventDto.Executed,
                    Included = eventDto.Included,
                    Pending = eventDto.Pending
                };
                // TODO: Morten - do check of this
                _storage.InitializeNewEvent(initialEventState);

                // #2. Then set our own fields accordingly
                _storage.EventId = eventDto.EventId;
                _storage.WorkflowId = eventDto.WorkflowId;
                _storage.Name = eventDto.Name;
                _storage.Roles = eventDto.Roles;
                _storage.Included = eventDto.Included;
                _storage.Pending = eventDto.Pending;
                _storage.Executed = eventDto.Executed;
                _storage.Inclusions = new HashSet<RelationToOtherEventModel>(eventDto.Inclusions.Select(addressDto => new RelationToOtherEventModel { EventID = addressDto.Id, Uri = addressDto.Uri }));
                _storage.Exclusions = new HashSet<RelationToOtherEventModel>(eventDto.Exclusions.Select(addressDto => new RelationToOtherEventModel { EventID = addressDto.Id, Uri = addressDto.Uri }));
                _storage.Conditions = new HashSet<RelationToOtherEventModel>(eventDto.Conditions.Select(addressDto => new RelationToOtherEventModel { EventID = addressDto.Id, Uri = addressDto.Uri }));
                _storage.Responses = new HashSet<RelationToOtherEventModel>(eventDto.Responses.Select(addressDto => new RelationToOtherEventModel { EventID = addressDto.Id, Uri = addressDto.Uri }));
                _storage.OwnUri = ownUri;
            }
            catch (Exception)
            {
                // if something goes wrong, we have to delete the event from the server again.
                serverCommunicator.DeleteEventFromServer().Wait();
                throw;
            }
        }

        public void DeleteEvent(string eventId)
        {
            // TODO: Implement locking check

            // Check if Event exists here
            if (!EventIdExists())
            {
                return;
            }

            // Attempt to delete Event from Server
            string workflowId = _storage.WorkflowId;
            IServerFromEvent serverCommunicator = new ServerCommunicator("http://flowit.azurewebsites.net/", eventId, workflowId);
            serverCommunicator.DeleteEventFromServer();

            // Delete Event from own Storage
            _storage.DeleteEvent();
        }

        public void ResetEvent(string eventId)
        {
            throw new NotImplementedException();
        }

        public EventDto GetEventDto(string eventId)
        {
            return new EventDto
            {
                EventId = _storage.EventId,
                WorkflowId = _storage.WorkflowId,
                Name = _storage.Name,
                Roles = _storage.Roles,
                Pending = _storage.Pending,
                Executed = _storage.Executed,
                Included = _storage.Included,
                Conditions = _storage.Conditions.Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri }),
                Exclusions = _storage.Exclusions.Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri }),
                Responses = _storage.Responses.Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri }),
                Inclusions = _storage.Inclusions.Select(model => new EventAddressDto { Id = model.EventID, Uri = model.Uri })
            };
        }

        public void Dispose()
        {
            _storage.Dispose();
        }

        private bool EventIdExists()
        {
            try
            {
                return _storage.Name != null;
            }
            catch (ApplicationException)
            {
                
                return false;
            }
        }
    }
}