using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Common.Exceptions;
using Event.Exceptions;
using Common.History;
using Event.Interfaces;
using Event.Models;
using Event.Models.UriClasses;

namespace Event.Storage
{
    /// <summary>
    /// EventStorage is the application-layer that rests on top of the actual storage-facility (a database)
    /// EventStorage implements IEventStorage-interface.
    /// </summary>
    public class EventStorage : IEventStorage, IEventHistoryStorage
    {
        private readonly IEventContext _context;

        /// <summary>
        /// Default constructor to be used in the application. Ties this instance to a hardcoded database-context. 
        /// </summary>
        public EventStorage()
        {
            _context = new EventContext();
        }
        /// <summary>
        /// Constructor used for dependency injection (used for testing purposes)
        /// </summary>
        /// <param name="context">Context to be used by EventStorage</param>
        public EventStorage(IEventContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initializes a new Event, based on the provided EventModel.
        /// </summary>
        /// <param name="eventModel"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when an Event with the same id already exists</exception>
        /// <exception cref="ArgumentNullException">Thrown when provided EventModel was null</exception>
        public async Task InitializeNewEvent(EventModel eventModel)
        {
            if (eventModel == null)
            {
                throw new ArgumentNullException("eventModel", "eventModel was null");
            }

            if (await Exists(eventModel.WorkflowId, eventModel.Id))
            {
                throw new EventExistsException();
            }

            _context.Events.Add(eventModel);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// DeleteEvent deletes the Event, belonging to the given workflowId and with the given eventId
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event to be deleted</param>
        /// <returns>Task</returns>
        /// <exception cref="ArgumentNullException">Will be thrown if either workflowId or eventId is null</exception>
        /// <exception cref="NotFoundException">Thrown if no event matches the identifying arguments</exception>
        public async Task DeleteEvent(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                // Discussion on why, we throw an exception, as opposed to saying "great, it's gone, in any case". 
                // http://lists.w3.org/Archives/Public/ietf-http-wg/2007JulSep/0347.html
                throw new NotFoundException();
            }

            _context.Conditions.RemoveRange(_context.Conditions.Where(c => c.WorkflowId == workflowId && c.EventId == eventId));
            _context.Exclusions.RemoveRange(_context.Exclusions.Where(e => e.WorkflowId == workflowId && e.EventId == eventId));
            _context.Inclusions.RemoveRange(_context.Inclusions.Where(i => i.WorkflowId == workflowId && i.EventId == eventId));
            _context.Responses.RemoveRange(_context.Responses.Where(r => r.WorkflowId == workflowId && r.EventId == eventId));

            _context.Events.Remove(_context.Events.Single(e => e.WorkflowId == workflowId && e.Id == eventId));

            await _context.SaveChangesAsync();
        }

        public async Task Reload(string workflowId, string eventId)
        {
            await _context.Entry(await _context.Events.SingleAsync(e => e.WorkflowId == workflowId && e.Id == eventId)).ReloadAsync();
        }

        #region Properties

        /// <summary>
        /// Tells whether an Event exists in the storage.
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns>True if the an Event with eventId exists at workflow with workflowId</returns>
        /// <exception cref="ArgumentNullException">Thrown if either eventId or workFlowId is null</exception>
        public async Task<bool> Exists(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            return await _context.Events.AnyAsync(e => e.WorkflowId == workflowId && e.Id == eventId);
        }

        /// <summary>
        /// Returns the URI-address of the Event belonging to the given workflowId and identified by eventId 
        /// </summary>
        /// <param name="workflowId">Identifies the workflow the event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if either eventId or workFlowId is null</exception>
        /// <exception cref="NotFoundException">Thrown if no event matches the identifying arguments</exception>
        public async Task<Uri> GetUri(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);
            return new Uri((await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).OwnUri);
        }

        // TODO: Is this method ever used?
        /// <summary>
        /// Sets the URI of the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event, whose Uri is to be set</param>
        /// <param name="uri">The uri, the Event's Uri should be set to</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Will be thrown if either of the input-arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if no event matches the identifying arguments</exception>
        public async Task SetUri(string workflowId, string eventId, Uri uri)
        {
            if (workflowId == null || eventId == null || uri == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            // Add replacing value
            _context.Events.Single(model => model.WorkflowId == workflowId && model.Id == eventId).OwnUri = uri.AbsoluteUri;
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Returns the name of an Event. 
        /// </summary>
        /// <param name="workflowId">Id of thw workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the provided arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if no Event exists with the given workflowId and EventId</exception>
        public async Task<string> GetName(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Name;
        }

        
        // TODO: Is this method ever used?
        public async Task SetName(string workflowId, string eventId, string name)
        {
            await EventIsInALegalState(workflowId, eventId);

            (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Name = name;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// GetRoles returns the Roles, that are allowed to execute an Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown when no Event matches the provided workflowId and eventId</exception>
        public async Task<IEnumerable<string>> GetRoles(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Roles.Select(role => role.Role);
        }

        // TODO: Is this method ever used? 
        public async Task SetRoles(string workflowId, string eventId, IEnumerable<string> value)
        {
            await EventIsInALegalState(workflowId, eventId);

            (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Roles = value.Select(role => new EventRoleModel { Role = role, WorkflowId = workflowId, EventId = eventId }).ToList();
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the Executed value for the specified event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the provided arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if the event does not exist in the storage</exception>
        public async Task<bool> GetExecuted(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Executed;
        }


        /// <summary>
        /// Sets the Executed value for the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="executedValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if either workflowId or eventId is null</exception>
        /// <exception cref="NotFoundException">Thrown if the event does not exist in the storage</exception>
        public async Task SetExecuted(string workflowId, string eventId, bool executedValue)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            try
            {
                (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Executed = executedValue;
            }
            catch (Exception)
            {
                throw new FailedToUpdateStateException();
            }
            
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the Included value of the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the provided arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if the event does not exist in the storage</exception>
        public async Task<bool> GetIncluded(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Included;
        }

        // TODO: This method needs testing. 
        /// <summary>
        /// Sets the Included value for the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="includedValue">The value that included should be set to</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the provided arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if the event does not exist in the storage</exception>
        public async Task SetIncluded(string workflowId, string eventId, bool includedValue)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            // TODO: Try-catch here...?
            await EventIsInALegalState(workflowId, eventId);

            var @event = (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId));
            
            @event.Included = includedValue;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the Included value of the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if the Event does not exist</exception>
        public async Task<bool> GetPending(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Pending;
        }

        /// <summary>
        /// Sets the Pending value for the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="pendingValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task SetPending(string workflowId, string eventId, bool pendingValue)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Pending = pendingValue;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the LockDto for the specified Event.
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if an Event with the specified ids does not exist.</exception>
        public async Task<LockDto> GetLockDto(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);
            // SingleOrDeafult will return either null or the actual single element in set. 
            var @event =
                await
                    _context.Events.SingleOrDefaultAsync(model => model.WorkflowId == workflowId && model.Id == eventId);
            if (@event.LockOwner == null) return null;      // TODO: With Exists() check above, this should be obsolete...? Right?
                                                            // TODO: From Mikael: Nope, Exists checks whether the event exists. This checks if there is a lockOwner set.
            return new LockDto
            {
                WorkflowId = @event.WorkflowId,
                Id = @event.Id,
                LockOwner = @event.LockOwner
            };
        }

        /// <summary>
        /// The setter for this property should not be used to unlock the Event. If setter is provided with a null-value
        /// an ArgumentNullException will be raised. Instead, use ClearLock()-method to remove any Lock on this Event.
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="lockOwner">Id of the lockowner</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if an Event with the specified ids does not exist.</exception>
        public async Task SetLock(string workflowId, string eventId, string lockOwner)
        {
            if (workflowId == null || eventId == null || lockOwner == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);
            
            var @event =
                await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId);
            if (@event.LockOwner != null)
            {
                throw new LockedException();
            }

            @event.LockOwner = lockOwner;

            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// This method should be used for unlocking an Event as opposed to using the setter for LockDto
        /// (Setter for LockDto will raise an ArgumentNullException if provided a null-value)
        /// The method simply removes all (should be either 1 or 0) LockDto element(s) held in database. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if an Event with the specified ids does not exist.</exception>
        public async Task ClearLock(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            var @event = await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId);

            @event.LockOwner = null;
            _context.Entry(@event).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the Condition-relations for the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<HashSet<RelationToOtherEventModel>> GetConditions(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }
            var dbset = _context.Conditions.Where(model => model.WorkflowId == workflowId && model.EventId == eventId);
            var hashSet = new HashSet<RelationToOtherEventModel>();

            foreach (var element in dbset)
            {
                hashSet.Add(new RelationToOtherEventModel
                {
                    Uri = new Uri(element.UriString),
                    EventId = element.ForeignEventId,
                    WorkflowId = element.WorkflowId
                });
            }
            return hashSet;
        }

        // TODO: Is this method ever used? Do we need it?
        /// <summary>
        /// Sets the Condition-relations on the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if an Event with the specified ids does not exist.</exception>
        public async Task SetConditions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            foreach (var uri in _context.Conditions.Where(model => model.WorkflowId == workflowId && model.EventId == eventId))
            {
                _context.Conditions.Remove(uri);
            }

            foreach (var element in value)
            {
                var uriToAdd = new ConditionUri
                {
                    UriString = element.Uri.AbsoluteUri,
                    ForeignEventId = element.EventId,
                    WorkflowId = element.WorkflowId,
                    EventId = eventId,
                };
                _context.Conditions.Add(uriToAdd);
            }
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// GetResponses returns a HashSet containing the response relations for the provided event.
        /// Notice, that this method will not return null, but may return an empty set.
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<HashSet<RelationToOtherEventModel>> GetResponses(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            var dbset = _context.Responses.Where(model => model.WorkflowId == workflowId && model.EventId == eventId);
            var hashSet = new HashSet<RelationToOtherEventModel>();

            foreach (var element in dbset)
            {
                hashSet.Add(new RelationToOtherEventModel
                {
                    Uri = new Uri(element.UriString),
                    EventId = element.ForeignEventId,
                    WorkflowId = element.WorkflowId
                });
            }

            return hashSet;
        }

        // TODO: Is this method ever used?
        /// <summary>
        /// Sets the Response-relations on the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="value">The response-relations that should be set on the specified Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if an Event with the specified ids does not exist.</exception>
        public async Task SetResponses(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }
            foreach (var uri in _context.Responses.Where(model => model.WorkflowId == workflowId && model.EventId == eventId))
            {
                _context.Responses.Remove(uri);
            }

            foreach (var uriToAdd in value.Select(element => new ResponseUri
            {
                UriString = element.Uri.AbsoluteUri,
                WorkflowId = element.WorkflowId,
                ForeignEventId = element.EventId,
                EventId = eventId
            }))
            {
                _context.Responses.Add(uriToAdd);
            }
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Returns the Exclusion-relations on the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<HashSet<RelationToOtherEventModel>> GetExclusions(string workflowId, string eventId)
        {
            if (workflowId == null ||eventId == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }


            var dbset = _context.Exclusions.Where(model => model.WorkflowId == workflowId && model.EventId == eventId);
            var hashSet = new HashSet<RelationToOtherEventModel>();

            foreach (var element in dbset)
            {
                hashSet.Add(new RelationToOtherEventModel
                {
                    Uri = new Uri(element.UriString),
                    EventId = element.ForeignEventId,
                    WorkflowId = element.WorkflowId
                });
            }

            return hashSet;
        }

        // TODO: Is this method ever used?
        /// <summary>
        /// Sets the Exclusion-relations on the specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="value">The Exclusion-relations that should be set on the specified Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        /// <exception cref="NotFoundException">Thrown if an Event with the specified ids does not exist.</exception>
        public async Task SetExclusions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            foreach (var uri in _context.Exclusions.Where(model => model.WorkflowId == workflowId && model.EventId == eventId))
            {
                _context.Exclusions.Remove(uri);
            }

            foreach (var uriToAdd in value.Select(element => new ExclusionUri
            {
                UriString = element.Uri.AbsoluteUri,
                WorkflowId = element.WorkflowId,
                ForeignEventId = element.EventId,
                EventId = eventId
            }))
            {
                _context.Exclusions.Add(uriToAdd);
            }
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// GetResponses returns a HashSet containing the inclusion relations for the provided event.
        /// Notice, that this method will not return null, but may return an empty set.
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null</exception>
        public async Task<HashSet<RelationToOtherEventModel>> GetInclusions(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }


            var dbset = _context.Inclusions.Where(model => model.WorkflowId == workflowId && model.EventId == eventId);
            var hashSet = new HashSet<RelationToOtherEventModel>();

            foreach (var element in dbset)
            {
                hashSet.Add(new RelationToOtherEventModel
                {
                    Uri = new Uri(element.UriString),
                    EventId = element.ForeignEventId,
                    WorkflowId = element.WorkflowId
                });
            }

            return hashSet;
        }

        // TODO: Is this method ever used?
        /// <summary>
        /// Sets the Inclusion-relations on the specified Event. Even if the Event has no Inclusion-relations
        /// an empty set (and not a Null-set) should be provided
        /// </summary>
        ///  <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event</param>
        /// <param name="value">The Inclusion-relations that should be set on the specified Event</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null.</exception>
        /// <exception cref="NotFoundException">Thrown if an Event with the specified ids does not exist.</exception>
        public async Task SetInclusions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value)
        {
            if (workflowId == null || eventId == null || value == null)
            {
                throw new ArgumentNullException();
            }
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            foreach (var uri in _context.Inclusions.Where(model => model.WorkflowId == workflowId && model.EventId == eventId))
            {
                _context.Inclusions.Remove(uri);
            }

            foreach (var uriToAdd in value.Select(element => new InclusionUri
            {
                UriString = element.Uri.AbsoluteUri,
                WorkflowId = element.WorkflowId,
                ForeignEventId = element.EventId,
                EventId = eventId
            }))
            {
                _context.Inclusions.Add(uriToAdd);
            }
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Disposes this context. (New Controllers are created for each HTTP-request, and hence, also disposed of
        /// when the HTTP-request is executed, and hence, this EventStorage is also disposed of)
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// EventIdentificationIsInALegalState makes two checks on EventIdentification-set,
        /// that when combined ensures that EventIdentification only has a single element. 
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null.</exception>
        private async Task EventIsInALegalState(string workflowId, string eventId)
        {
            if (workflowId == null || eventId == null)
            {
                throw new ArgumentNullException();
            }

            // Check that there's currently only a single element in database
            if (await _context.Events.CountAsync(model => model.WorkflowId == workflowId && model.Id == eventId) > 1)
            {
                throw new IllegalStorageStateException();
            }

            if (!await _context.Events.AnyAsync(model => model.WorkflowId == workflowId && model.Id == eventId))
            {
                throw new IllegalStorageStateException();
            }
        }
        #endregion

        /// <summary>
        /// Saves the given historyModel to storage.
        /// </summary>
        /// <param name="toSave">The history that is to be saved</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">Thrown if the specified Event does not exist</exception>
        public async Task SaveHistory(HistoryModel toSave)
        {
            if (!await Exists(toSave.WorkflowId, toSave.EventId))
            {
                throw new NotFoundException();
            }

            _context.History.Add(toSave);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the History for a specified Event
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event, whose history is to be retrieved</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">Thrown if the specified Event does not exist</exception>
        public async Task<IQueryable<HistoryModel>> GetHistoryForEvent(string workflowId, string eventId)
        {
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            return _context.History.Where(h => h.EventId == eventId && h.WorkflowId == workflowId);
        }
    }
}