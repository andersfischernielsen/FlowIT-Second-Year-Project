using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        private EventContext _context;

        // TODO: Discuss: Do we need to dependency-inject the context in here, for unit-testing purposes?
        public EventStorage(string eventId)
        {

            _context = new EventContext();
        }


        #region Properties
        public Uri OwnUri
        {
            get
            {
                // We should only have one of these objects in the database
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than one EventIdentification object in database");
                }

                var eventIdPackage = _context.EventIdentification.SingleOrDefault();
                if (eventIdPackage == null)
                {
                    return null;
                }

                return new Uri(eventIdPackage.OwnUri);
            }
            set
            {
                // Check that there's currently only a single element in database
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException(
                        "More than a single EventIdentification element in database in Event");
                }

                if (!_context.EventIdentification.Any())
                {
                    throw new ApplicationException("EventIdentification was not initialized in Event");
                }

                // Add replacing value
                _context.EventIdentification.Single().OwnUri = value.AbsoluteUri;
                _context.SaveChanges();

            }
        }

        public string WorkflowId
        {
            get
            {
                EventIdentificationIsInALegalState();

                var eventIdentificationPackage = _context.EventIdentification.FirstOrDefault();
                if (eventIdentificationPackage == null)
                {
                    return null;
                }
                return eventIdentificationPackage.WorkflowId;

            }
            set
            {
                EventIdentificationIsInALegalState();

                _context.EventIdentification.Single().WorkflowId = value;
                _context.SaveChanges();

                _context.EventIdentification.Single().WorkflowId = value;
                _context.SaveChanges();
            }
        }

        public string EventId
        {
            get
            {
                EventIdentificationIsInALegalState();

                var eventIdentificationPackage = _context.EventIdentification.FirstOrDefault();
                if (eventIdentificationPackage == null)
                {
                    return null;
                }
                return eventIdentificationPackage.EventId;
            }
            set
            {
                EventIdentificationIsInALegalState();

                _context.EventIdentification.Single().EventId = value;
                _context.SaveChanges();


                _context.EventIdentification.Single().EventId = value;
                _context.SaveChanges();
            }
        }

        public string Name
        {
            get
            {
                EventIdentificationIsInALegalState();
                return _context.EventIdentification.Single().Name;
            }
            set
            {
                EventIdentificationIsInALegalState();

                _context.EventIdentification.Single().Name = value;
                _context.SaveChanges();
            }
        }

        public string Role
        {
            get
            {
                EventIdentificationIsInALegalState();

                return _context.EventIdentification.Single().Role;
            }
            set
            {
                EventIdentificationIsInALegalState();

                _context.EventIdentification.Single().Role = value;
                _context.SaveChanges();
            }
        }

        public bool Executed
        {
            get
            {
                EventStateIsInALegalState();

                return _context.EventState.Single().Executed;
            }
            set
            {
                EventStateIsInALegalState();

                _context.EventState.Single().Executed = value;
                _context.SaveChanges();
            }
        }

        public bool Included
        {
            get
            {
                EventStateIsInALegalState();

                return _context.EventState.Single().Included;
            }
            set
            {
                EventStateIsInALegalState();

                _context.EventState.Single().Included = value;
                _context.SaveChanges();
            }
        }

        public bool Pending
        {
            get
            {
                EventStateIsInALegalState();

                return _context.EventState.Single().Pending;
            }
            set
            {
                EventStateIsInALegalState();

                _context.EventState.Single().Pending = value;
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// The setter for this property should not be used to unlock the Event. If setter is provided with a null-value
        /// an ArgumentNullException will be raised. Instead, use ClearLock()-method to remove any Lock on this Event.  
        /// </summary>
        public LockDto LockDto
        {
            get
            {
                // Check to ensure there's only a single LockDto held in database
                if (_context.LockDto.Count() > 1)
                {
                    throw new ApplicationException("Illegal state in Event: More than a " +
                                                   "single LockDto was held in database");
                }
                // SingleOrDeafult will return either null or the actual single element in set. 
                return _context.LockDto.SingleOrDefault();
            }
            set
            {
                // Check that there is no more than a single element in LockDto set
                if (_context.LockDto.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in LockDto");
                }

                // Remove current LockDto (should be either only a single element or no element at all
                foreach (var element in _context.LockDto)
                {
                    _context.LockDto.Remove(element);
                }

                if (value == null)
                {
                    throw new ArgumentNullException("value", "The provided LockDto was null. To unlock Event, " +
                                                            "see documentation");
                }

                _context.LockDto.Add(value);
                _context.SaveChanges();
            }
        }


        /// <summary>
        /// This method should be used for unlocking an Event as opposed to using the setter for LockDto
        /// (Setter for LockDto will raise an ArgumentNullException if provided a null-value)
        /// The method simply removes all (should be either 1 or 0) LockDto element(s) held in database. 
        /// </summary>
        public void ClearLock()
        {
            // Check that LockDto set is in a legal state
            if (_context.LockDto.Count() > 1)
            {
                throw new ApplicationException("Illegal state in Event: LockDto set contains more than a single element");
            }

            // Clear the single LockDto-element 
            foreach (var lockDto in _context.LockDto)
            {
                _context.LockDto.Remove(lockDto);
            }
            _context.SaveChanges();
        }

        public HashSet<Uri> Conditions
        {
            get
            {
                // No need to do zero or ">1" count check here; that is perfectly legal
                var dbset = _context.Conditions;
                var hashSet = new HashSet<Uri>();

                foreach (var element in dbset)
                {
                    hashSet.Add(new Uri(element.UriString));
                }

                return hashSet;
            }
            set
            {
                // Reset current list
                foreach (var uri in _context.Conditions)
                {
                    _context.Conditions.Remove(uri);
                }

                // Add replacing values
                foreach (var element in value)
                {
                    var uriToAdd = new ConditionUri() { UriString = element.AbsoluteUri };
                    _context.Conditions.Add(uriToAdd);
                }

                _context.SaveChanges();
            }
        }

        public HashSet<Uri> Responses
        {
            get
            {
                var dbset = _context.Responses;
                var hashSet = new HashSet<Uri>();

                foreach (var element in dbset)
                {
                    hashSet.Add(new Uri(element.UriString));
                }

                return hashSet;
            }
            set
            {
                // Remove current content 
                foreach (var uri in _context.Responses)
                {
                    _context.Responses.Remove(uri);
                }

                // Add replacing content
                foreach (var element in value)
                {
                    var uriToAdd = new ResponseUri() { UriString = element.AbsoluteUri };
                    _context.Responses.Add(uriToAdd);
                }

                _context.SaveChanges();
            }
        }

        public HashSet<Uri> Exclusions
        {
            get
            {
                var dbset = _context.Exclusions;
                var hashSet = new HashSet<Uri>();

                foreach (var element in dbset)
                {
                    hashSet.Add(new Uri(element.UriString));
                }

                return hashSet;
            }
            set
            {
                // Remove current content
                foreach (var uri in _context.Exclusions)
                {
                    _context.Exclusions.Remove(uri);
                }

                // Add replacing values
                foreach (var element in value)
                {
                    var uriToAdd = new ExclusionUri() { UriString = element.AbsoluteUri };
                    _context.Exclusions.Add(uriToAdd);
                }

                _context.SaveChanges();
            }
        }

        public HashSet<Uri> Inclusions
        {
            get
            {
                var dbset = _context.Inclusions;
                var hashSet = new HashSet<Uri>();

                foreach (var element in dbset)
                {
                    hashSet.Add(new Uri(element.UriString));
                }

                return hashSet;
            }
            set
            {
                foreach (var uri in _context.Inclusions)
                {
                    _context.Inclusions.Remove(uri);
                }

                foreach (var element in value)
                {
                    var uriToAdd = new InclusionUri() { UriString = element.AbsoluteUri };
                    _context.Inclusions.Add(uriToAdd);
                }
                _context.SaveChanges();
            }
        }

        public ICollection<EventUriIdMapping> EventUriIdMappings
        {
            get
            {
                return _context.EventUriIdMappings.ToList();
            }
            set
            {
                // Remove current entries
                foreach (var element in _context.EventUriIdMappings)
                {
                    _context.EventUriIdMappings.Remove(element);
                }

                // Add replacing entries
                foreach (var element in value)
                {
                    _context.EventUriIdMappings.Add(element);
                }
                _context.SaveChanges();
            }
        }
        #endregion

        #region Public methods
        public Uri GetUriFromId(string id)
        {
            // TODO: Discuss: Use Task instead and await here?: Update IEventStorage to reflect
            var uri = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
            if (uri == null) return null;
            return new Uri(uri.Uri);
        }

        /// <summary>
        /// Given an URI-object (representing another Event's URI) this method returns the related id.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public string GetIdFromUri(Uri endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint","Supplied argument was null");
            }

            var result = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Uri.Equals(endPoint.AbsoluteUri)).Result;
            if (result == null) return null;
            return result.Id;
        }

        /// <summary>
        /// RemoveIdAndUri will delete the entry (that represents an Event by an Id and a Uri),
        /// that is held in the database.
        /// </summary>
        /// <param name="id">Id of the Event, whose entry is to be removed in the databse</param>
        public void RemoveIdAndUri(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id","Supplied argument was null");
            }

            var toRemove = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
            if (toRemove == null) return;
            _context.EventUriIdMappings.Remove(toRemove);
            _context.SaveChanges();
        }

        // TODO: Discuss: Is this method also intended to be used for updating an existing entry? In that
        // TODO: case the current implementation is faulty...because it currently **adds** a new entry
        /// <summary>
        /// StoreIdAndUri adds an entry to the database. The entry represents an Event (by the Event's Id and Uri)
        /// </summary>
        /// <param name="id">The id of the Event, that an entry is to be added for in the database</param>
        /// <param name="endPoint"></param>
        public void StoreIdAndUri(string id, Uri endPoint)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id", "id was null in StoreIdAndUri");
            }
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint","endPoint was null in StoreIdAndUri");
            }

            var eventUriIdMapping = new EventUriIdMapping() { Id = id, Uri = endPoint.AbsolutePath };
            _context.EventUriIdMappings.Add(eventUriIdMapping);

            _context.SaveChanges();

        }

        /// <summary>
        /// IdExists checks whether the database currently holds an entry matching the supplied id. 
        /// </summary>
        /// <param name="id">Id of the Event to check for existence</param>
        /// <returns></returns>
        public bool IdExists(string id)
        {
            var result = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
            return result != null;

        }

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
        private void EventIdentificationIsInALegalState()
        {
            // Check that there's currently only a single element in database
            if (_context.EventIdentification.Count() > 1)
            {
                throw new ApplicationException(
                    "More than a single EventIdentification element in database-set in Event");
            }

            if (!_context.EventIdentification.Any())
            {
                throw new ApplicationException("EventIdentification was not initialized in Event." +
                                               "Count was zero");
            }

        }

        /// <summary>
        /// EventStateIsInALegalState makes two checks on EventState-set,
        /// that when combined ensures that EventState only has a single element. 
        /// </summary>
        private void EventStateIsInALegalState()
        {
            // Check that there is no more than a single element in EventState
            if (_context.EventState.Count() > 1)
            {
                throw new ApplicationException("More than a single element in EventState set");
            }
            if (!_context.EventState.Any())
            {
                throw new ApplicationException("EventState was not initialized in Event");
            }
        }
        #endregion
    }
}