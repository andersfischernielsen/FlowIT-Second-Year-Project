using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Common.Exceptions;
using Event.Interfaces;
using Event.Models;
using Event.Models.UriClasses;

namespace Event.Storage
{
    /// <summary>
    /// EventStorage is the application-layer that rests on top of the actual storage-facility (a database)
    /// EventStorage implements IEventStorage-interface.
    /// </summary>
    public class EventStorage : IEventStorage
    {
        private readonly IEventContext _context;

        public EventStorage()
        {
            _context = new EventContext();
        }
        public EventStorage(IEventContext context)
        {
            _context = context;
        }

        public async Task InitializeNewEvent(EventModel eventModel)
        {
            // Todo: Merge EventIdentification and EventState and use Exists(eventId) instead.
            if (await Exists(eventModel.WorkflowId, eventModel.Id))
            {
                throw new InvalidOperationException("The EventId is already existing");
            }

            _context.Events.Add(eventModel);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteEvent(string workflowId, string eventId)
        {
            if (!await Exists(workflowId, eventId))
            {
                throw new InvalidOperationException("The EventId does not exist");
            }

            _context.Conditions.RemoveRange(_context.Conditions.Where(c => c.WorkflowId == workflowId && c.EventId == eventId));
            _context.Exclusions.RemoveRange(_context.Exclusions.Where(e => e.WorkflowId == workflowId && e.EventId == eventId));
            _context.Inclusions.RemoveRange(_context.Inclusions.Where(i => i.WorkflowId == workflowId && i.EventId == eventId));
            _context.Responses.RemoveRange(_context.Responses.Where(r => r.WorkflowId == workflowId && r.EventId == eventId));

            _context.Events.Remove(_context.Events.Single(e => e.WorkflowId == workflowId && e.Id == eventId));

            await _context.SaveChangesAsync();
        }


        #region Properties

        public async Task<bool> Exists(string workflowId, string eventId)
        {
            return await _context.Events.AnyAsync(e => e.WorkflowId == workflowId && e.Id == eventId);
        }

        public async Task<Uri> GetUri(string workflowId, string eventId)
        {
            await EventIsInALegalState(workflowId, eventId);
            return new Uri((await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).OwnUri);
        }

        public async Task SetUri(string workflowId, string eventId, Uri value)
        {
            await EventIsInALegalState(workflowId, eventId);

            // Add replacing value
            _context.Events.Single(model => model.WorkflowId == workflowId && model.Id == eventId).OwnUri = value.AbsoluteUri;
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetName(string workflowId, string eventId)
        {
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Name;
        }

        public async Task SetName(string workflowId, string eventId, string value)
        {
            await EventIsInALegalState(workflowId, eventId);

            (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Name = value;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetRoles(string workflowId, string eventId)
        {
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Roles.Select(role => role.Role);
        }

        public async Task SetRoles(string workflowId, string eventId, IEnumerable<string> value)
        {
            await EventIsInALegalState(workflowId, eventId);

            (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Roles = value.Select(role => new EventRoleModel { Role = role, WorkflowId = workflowId, EventId = eventId }).ToList();
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetExecuted(string workflowId, string eventId)
        {
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Executed;
        }

        public async Task SetExecuted(string workflowId, string eventId, bool value)
        {
            await EventIsInALegalState(workflowId, eventId);

            (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Executed = value;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetIncluded(string workflowId, string eventId)
        {
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Included;
        }
        public async Task SetIncluded(string workflowId, string eventId, bool value)
        {
            await EventIsInALegalState(workflowId, eventId);

            (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Included = value;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetPending(string workflowId, string eventId)
        {
            if (!await Exists(workflowId, eventId))
            {
                throw new NotFoundException();
            }

            await EventIsInALegalState(workflowId, eventId);

            return (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Pending;
        }
        public async Task SetPending(string workflowId, string eventId, bool value)
        {
            await EventIsInALegalState(workflowId, eventId);

            (await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId)).Pending = value;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// The setter for this property should not be used to unlock the Event. If setter is provided with a null-value
        /// an ArgumentNullException will be raised. Instead, use ClearLock()-method to remove any Lock on this Event.  
        /// </summary>
        public async Task<LockDto> GetLockDto(string workflowId, string eventId)
        {
            await EventIsInALegalState(workflowId, eventId);
            // SingleOrDeafult will return either null or the actual single element in set. 
            var @event =
                await
                    _context.Events.SingleOrDefaultAsync(model => model.WorkflowId == workflowId && model.Id == eventId);
            if (@event.LockOwner == null) return null;
            return new LockDto
            {
                WorkflowId = @event.WorkflowId,
                Id = @event.Id,
                LockOwner = @event.LockOwner
            };
        }
        public async Task SetLock(string workflowId, string eventId, string lockOwner)
        {
            await EventIsInALegalState(workflowId, eventId);
            if (lockOwner == null)
            {
                throw new ArgumentNullException("lockOwner", "The provided lockOwner was null. To unlock Event, " +
                                                        "see documentation");
            }
            var @event =
                await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId);
            if (@event.LockOwner != null)
            {
                throw new ApplicationException("There already exists a lock on this event");
            }

            @event.LockOwner = lockOwner;

            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// This method should be used for unlocking an Event as opposed to using the setter for LockDto
        /// (Setter for LockDto will raise an ArgumentNullException if provided a null-value)
        /// The method simply removes all (should be either 1 or 0) LockDto element(s) held in database. 
        /// </summary>
        public async Task ClearLock(string workflowId, string eventId)
        {
            await EventIsInALegalState(workflowId, eventId);

            var @event =
                await _context.Events.SingleAsync(model => model.WorkflowId == workflowId && model.Id == eventId);

            @event.LockOwner = null;

            await _context.SaveChangesAsync();
        }

        public HashSet<RelationToOtherEventModel> GetConditions(string workflowId, string eventId)
        {
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
        public async Task SetConditions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value)
        {
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
        /// <param name="workflowId"></param>
        /// <param name="eventId">Id of the Event, for which you wish to retrieve response-relations</param>
        /// <returns></returns>
        public HashSet<RelationToOtherEventModel> GetResponses(string workflowId, string eventId)
        {
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
        public async Task SetResponses(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value)
        {
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
        public HashSet<RelationToOtherEventModel> GetExclusions(string workflowId, string eventId)
        {
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
        public async Task SetExclusions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value)
        {
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
        /// <param name="workflowId"></param>
        /// <param name="eventId">Id of the Event, for which you wish to retrieve inclusion-relations</param>
        /// <returns></returns>
        public HashSet<RelationToOtherEventModel> GetInclusions(string workflowId, string eventId)
        {
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
        public async Task SetInclusions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value)
        {
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
        private async Task EventIsInALegalState(string workflowId, string eventId)
        {
            // Check that there's currently only a single element in database
            if (await _context.Events.CountAsync(model => model.WorkflowId == workflowId && model.Id == eventId) > 1)
            {
                throw new ApplicationException(
                    "More than a single EventIdentification element in database-set in Event");
            }


            if (!await _context.Events.AnyAsync(model => model.WorkflowId == workflowId && model.Id == eventId))
            {
                throw new ApplicationException("EventIdentification was not initialized in Event." +
                                               "Count was zero");
            }
        }
        #endregion
    }
}