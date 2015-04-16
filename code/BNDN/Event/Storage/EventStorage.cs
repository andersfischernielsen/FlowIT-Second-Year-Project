using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
        private IEventContext _context;

        public EventStorage(IEventContext context)
        {
            _context = context;
        }

        public async Task InitializeNewEvent(InitialEventState initialEventState)
        {
            if (await _context.EventIdentification.AnyAsync(model => model.Id == initialEventState.EventId))
            {
                throw new InvalidOperationException("The EventId is already existing");
            }
            if (await _context.EventState.AnyAsync(model => model.Id == initialEventState.EventId))
            {
                throw new InvalidOperationException("The EventId is already existing");
            }
            _context.EventIdentification.Add(new EventIdentificationModel { Id = initialEventState.EventId, Roles = new List<EventRoleModel>() });
            _context.EventState.Add(new EventStateModel { Id = initialEventState.EventId });
            _context.InitialEventState.Add(initialEventState);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEvent(string eventId)
        {
            if (!await _context.EventIdentification.AnyAsync(model => model.Id == eventId))
            {
                throw new InvalidOperationException("The EventId does not exist");
            }
            if (!await _context.EventState.AnyAsync(model => model.Id == eventId))
            {
                throw new InvalidOperationException("The EventId does not exist");
            }

            _context.Conditions.RemoveRange(_context.Conditions.Where(ei => ei.EventIdentificationModelId == eventId));
            _context.Exclusions.RemoveRange(_context.Exclusions.Where(ei => ei.EventIdentificationModelId == eventId));
            _context.Inclusions.RemoveRange(_context.Inclusions.Where(ei => ei.EventIdentificationModelId == eventId));
            _context.Responses.RemoveRange(_context.Responses.Where(ei => ei.EventIdentificationModelId == eventId));

            _context.EventState.Remove(_context.EventState.Single(ei => ei.Id == eventId));
            _context.EventIdentification.Remove(_context.EventIdentification.Single(ei => ei.Id == eventId));

            await _context.SaveChangesAsync();
        }


        #region Properties

        public async  Task<bool> Exists(string eventId)
        {
            return await _context.EventIdentification.AnyAsync(ei => ei.Id == eventId);
        }

        public async Task<Uri> GetUri(string eventId)
        {
            await EventIdentificationIsInALegalState(eventId);
            return new Uri((await _context.EventIdentification.SingleAsync(model => model.Id == eventId)).OwnUri);
        }

        public async Task SetUri(string eventId, Uri value)
        {
            await EventIdentificationIsInALegalState(eventId);

            // Add replacing value
            _context.EventIdentification.Single(model => model.Id == eventId).OwnUri = value.AbsoluteUri;
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetWorkflowId(string eventId)
        {
            await EventIdentificationIsInALegalState(eventId);
            return (await _context.EventIdentification.SingleAsync(model => model.Id == eventId)).WorkflowId;
        }

        public async Task SetWorkflowId(string eventId, string value)
        {
            await EventIdentificationIsInALegalState(eventId);

            (await _context.EventIdentification.SingleAsync(model => model.Id == eventId)).WorkflowId = value;
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetName(string eventId)
        {
            await EventIdentificationIsInALegalState(eventId);
            return (await _context.EventIdentification.SingleAsync(model => model.Id == eventId)).Name;
        }

        public async Task SetName(string eventId, string value)
        {
            await EventIdentificationIsInALegalState(eventId);

            (await _context.EventIdentification.SingleAsync(model => model.Id == eventId)).Name = value;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetRoles(string eventId)
        {
            await EventIdentificationIsInALegalState(eventId);

            return (await _context.EventIdentification.SingleAsync(model => model.Id == eventId)).Roles.Select(role => role.Role);
        }

        public async Task SetRoles(string eventId, IEnumerable<string> value)
        {
            await EventIdentificationIsInALegalState(eventId);

            (await _context.EventIdentification.SingleAsync(model => model.Id == eventId)).Roles = value.Select(role => new EventRoleModel { Role = role, EventId = eventId }).ToList();
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetExecuted(string eventId)
        {
            await EventStateIsInALegalState(eventId);

            return (await _context.EventState.SingleAsync(model => model.Id == eventId)).Executed;
        }

        public async Task SetExecuted(string eventId, bool value)
        {
            await EventStateIsInALegalState(eventId);

            (await _context.EventState.SingleAsync(model => model.Id == eventId)).Executed = value;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetIncluded(string eventId)
        {
            await EventStateIsInALegalState(eventId);

            return (await _context.EventState.SingleAsync(model => model.Id == eventId)).Included;
        }
        public async Task SetIncluded(string eventId, bool value)
        {
            await EventStateIsInALegalState(eventId);

            (await _context.EventState.SingleAsync(model => model.Id == eventId)).Included = value;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetPending(string eventId)
        {
            await EventStateIsInALegalState(eventId);

            return (await _context.EventState.SingleAsync(model => model.Id == eventId)).Pending;
        }
        public async Task SetPending(string eventId, bool value)
        {
            await EventStateIsInALegalState(eventId);

            (await _context.EventState.SingleAsync(model => model.Id == eventId)).Pending = value;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// The setter for this property should not be used to unlock the Event. If setter is provided with a null-value
        /// an ArgumentNullException will be raised. Instead, use ClearLock()-method to remove any Lock on this Event.  
        /// </summary>
        public async Task<LockDto> GetLockDto(string eventId)
        {
            await EventLockIsInALegalState(eventId);
            // SingleOrDeafult will return either null or the actual single element in set. 
            return await _context.LockDto.SingleOrDefaultAsync(model => model.Id == eventId);
        }
        public async Task SetLockDto(string eventId, LockDto value)
        {
            await EventLockIsInALegalState(eventId);
            if (await _context.LockDto.AnyAsync(model => model.Id == eventId))
            {
                throw new ApplicationException("There already exists a lock on this event");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value", "The provided LockDto was null. To unlock Event, " +
                                                        "see documentation");
            }

            // Remove current LockDto (should be either only a single element or no element at all
            // Should not be neccesary.
            foreach (var element in _context.LockDto.Where(model => model.Id == eventId))
            {
                _context.LockDto.Remove(element);
            }
            //Todo: Maybe this should not be done here - but this is the safest way.
            var theLock = value;
            theLock.Id = eventId;

            _context.LockDto.Add(theLock);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// This method should be used for unlocking an Event as opposed to using the setter for LockDto
        /// (Setter for LockDto will raise an ArgumentNullException if provided a null-value)
        /// The method simply removes all (should be either 1 or 0) LockDto element(s) held in database. 
        /// </summary>
        public async Task ClearLock(string eventId)
        {
            await EventLockIsInALegalState(eventId);

            // Clear the single LockDto-element 
            foreach (var lockDto in _context.LockDto.Where(model => model.Id == eventId))
            {
                _context.LockDto.Remove(lockDto);
            }
            await _context.SaveChangesAsync();
        }

        public HashSet<RelationToOtherEventModel> GetConditions(string eventId)
        {
            var dbset = _context.Conditions.Where(model => model.EventIdentificationModelId == eventId);

            return new HashSet<RelationToOtherEventModel>(dbset.Select(element => new RelationToOtherEventModel
            {
                Uri = new Uri(element.UriString),
                EventID = element.EventId
            }));
        }
        public async Task SetConditions(string eventId, HashSet<RelationToOtherEventModel> value)
        {
            foreach (var uri in _context.Conditions.Where(model => model.EventIdentificationModelId == eventId))
            {
                _context.Conditions.Remove(uri);
            }

            foreach (var element in value)
            {
                var uriToAdd = new ConditionUri()
                {
                    UriString = element.Uri.AbsoluteUri,
                    EventId = element.EventID,
                    EventIdentificationModelId = eventId
                };
                _context.Conditions.Add(uriToAdd);
            }
            await _context.SaveChangesAsync();
        }
        public HashSet<RelationToOtherEventModel> GetResponses(string eventId)
        {
            var dbset = _context.Responses.Where(model => model.EventIdentificationModelId == eventId);
            var hashSet = new HashSet<RelationToOtherEventModel>();

            foreach (var element in dbset)
            {
                hashSet.Add(new RelationToOtherEventModel
                {
                    Uri = new Uri(element.UriString),
                    EventID = element.EventId
                });
            }

            return hashSet;
        }
        public async Task SetResponses(string eventId, HashSet<RelationToOtherEventModel> value)
        {
            foreach (var uri in _context.Responses.Where(model => model.EventIdentificationModelId == eventId))
            {
                _context.Responses.Remove(uri);
            }

            foreach (var uriToAdd in value.Select(element => new ResponseUri
            {
                UriString = element.Uri.AbsoluteUri,
                EventId = element.EventID,
                EventIdentificationModelId = eventId
            }))
            {
                _context.Responses.Add(uriToAdd);
            }
            await _context.SaveChangesAsync();
        }
        public HashSet<RelationToOtherEventModel> GetExclusions(string eventId)
        {
            var dbset = _context.Exclusions.Where(model => model.EventIdentificationModelId == eventId);
            var hashSet = new HashSet<RelationToOtherEventModel>();

            foreach (var element in dbset)
            {
                hashSet.Add(new RelationToOtherEventModel
                {
                    Uri = new Uri(element.UriString),
                    EventID = element.EventId
                });
            }

            return hashSet;
        }
        public async Task SetExclusions(string eventId, HashSet<RelationToOtherEventModel> value)
        {
            foreach (var uri in _context.Exclusions.Where(model => model.EventIdentificationModelId == eventId))
            {
                _context.Exclusions.Remove(uri);
            }

            foreach (var uriToAdd in value.Select(element => new ExclusionUri
            {
                UriString = element.Uri.AbsoluteUri,
                EventId = element.EventID,
                EventIdentificationModelId = eventId
            }))
            {
                _context.Exclusions.Add(uriToAdd);
            }
            await _context.SaveChangesAsync();
        }
        public HashSet<RelationToOtherEventModel> GetInclusions(string eventId)
        {
            var dbset = _context.Inclusions.Where(model => model.EventIdentificationModelId == eventId);
            var hashSet = new HashSet<RelationToOtherEventModel>();

            foreach (var element in dbset)
            {
                hashSet.Add(new RelationToOtherEventModel
                {
                    Uri = new Uri(element.UriString),
                    EventID = element.EventId
                });
            }

            return hashSet;
        }
        public async Task SetInclusions(string eventId, HashSet<RelationToOtherEventModel> value)
        {
            foreach (var uri in _context.Inclusions.Where(model => model.EventIdentificationModelId == eventId))
            {
                _context.Inclusions.Remove(uri);
            }

            foreach (var uriToAdd in value.Select(element => new InclusionUri
            {
                UriString = element.Uri.AbsoluteUri,
                EventId = element.EventID,
                EventIdentificationModelId = eventId
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
            _context = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// EventIdentificationIsInALegalState makes two checks on EventIdentification-set,
        /// that when combined ensures that EventIdentification only has a single element. 
        /// </summary>
        private async Task EventIdentificationIsInALegalState(string eventId)
        {
            // Check that there's currently only a single element in database
            if (await _context.EventIdentification.CountAsync(model => model.Id == eventId) > 1)
            {
                throw new ApplicationException(
                    "More than a single EventIdentification element in database-set in Event");
            }


            if (!await _context.EventIdentification.AnyAsync(model => model.Id == eventId))
            {
                throw new ApplicationException("EventIdentification was not initialized in Event." +
                                               "Count was zero");
            }
        }

        /// <summary>
        /// EventLockIsInALegalState makes two checks on LockDto-set,
        /// that when combined ensures that LockDto only has a single element. 
        /// </summary>
        private async Task EventLockIsInALegalState(string eventId)
        {
            // Check that there's currently only a single element in database
            if (await _context.LockDto.CountAsync(model => model.Id == eventId) > 1)
            {
                throw new ApplicationException(
                    "More than a single Lock element in database-set in Event");
            }
        }

        /// <summary>
        /// EventStateIsInALegalState makes two checks on EventState-set,
        /// that when combined ensures that EventState only has a single element. 
        /// </summary>
        private async Task EventStateIsInALegalState(string eventId)
        {
            // Check that there is no more than a single element in EventState
            if (await _context.EventState.CountAsync(model => model.Id == eventId) > 1)
            {
                throw new ApplicationException("More than a single element in EventState set");
            }
            if (!await _context.EventState.AnyAsync(model => model.Id == eventId))
            {
                throw new ApplicationException("EventState was not initialized in Event");
            }
        }
        #endregion
    }
}