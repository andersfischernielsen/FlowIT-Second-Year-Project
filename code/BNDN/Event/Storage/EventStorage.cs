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
        private IEventContext _context;

        public EventStorage(string eventId, IEventContext context)
        {
            EventId = eventId;
            _context = context;


            
            if (!_context.EventIdentification.Any(model => model.Id == eventId))
            {
                _context.EventIdentification.Add(new EventIdentificationModel() { Id = EventId });
            }
            if (!_context.EventState.Any(model => model.Id == eventId))
            {
                _context.EventState.Add(new EventStateModel() { Id = EventId });
            }
            _context.SaveChanges();
        }


        #region Properties
        public Uri OwnUri
        {
            get
            {
                EventIdentificationIsInALegalState();
                return new Uri(_context.EventIdentification.Single(model => model.Id == EventId).OwnUri);
            }
            set
            {
                EventIdentificationIsInALegalState();

                // Add replacing value
                _context.EventIdentification.Single(model => model.Id == EventId).OwnUri = value.AbsoluteUri;
                _context.SaveChanges();

            }
        }

        public string WorkflowId
        {
            get
            {
                EventIdentificationIsInALegalState();
                return _context.EventIdentification.Single(model => model.Id == EventId).WorkflowId;
            }
            set
            {
                EventIdentificationIsInALegalState();

                _context.EventIdentification.Single(model => model.Id == EventId).WorkflowId = value;
                _context.SaveChanges();
            }
        }

        public string EventId { get; set; }

        public string Name
        {
            get
            {
                EventIdentificationIsInALegalState();
                return _context.EventIdentification.Single(model => model.Id == EventId).Name;
            }
            set
            {
                EventIdentificationIsInALegalState();

                _context.EventIdentification.Single(model => model.Id == EventId).Name = value;
                _context.SaveChanges();
            }
        }

        public string Role
        {
            get
            {
                EventIdentificationIsInALegalState();

                return _context.EventIdentification.Single(model => model.Id == EventId).Role;
            }
            set
            {
                EventIdentificationIsInALegalState();

                _context.EventIdentification.Single(model => model.Id == EventId).Role = value;
                _context.SaveChanges();
            }
        }

        public bool Executed
        {
            get
            {
                EventStateIsInALegalState();

                return _context.EventState.Single(model => model.Id == EventId).Executed;
            }
            set
            {
                EventStateIsInALegalState();

                _context.EventState.Single(model => model.Id == EventId).Executed = value;
                _context.SaveChanges();
            }
        }

        public bool Included
        {
            get
            {
                EventStateIsInALegalState();

                return _context.EventState.Single(model => model.Id == EventId).Included;
            }
            set
            {
                EventStateIsInALegalState();

                _context.EventState.Single(model => model.Id == EventId).Included = value;
                _context.SaveChanges();
            }
        }

        public bool Pending
        {
            get
            {
                EventStateIsInALegalState();

                return _context.EventState.Single(model => model.Id == EventId).Pending;
            }
            set
            {
                EventStateIsInALegalState();

                _context.EventState.Single(model => model.Id == EventId).Pending = value;
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
                EventLockIsInALegalState();
                // SingleOrDeafult will return either null or the actual single element in set. 
                return _context.LockDto.SingleOrDefault(model => model.Id == EventId);
            }
            set
            {
                EventLockIsInALegalState();
                if (_context.LockDto.Any(model => model.Id == EventId))
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
                foreach (var element in _context.LockDto.Where(model => model.Id == EventId))
                {
                    _context.LockDto.Remove(element);
                }
                //Todo: Maybe this should not be done here - but this is the safest way.
                var theLock = value;
                theLock.Id = EventId;

                _context.LockDto.Add(theLock);
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
            EventLockIsInALegalState();

            // Clear the single LockDto-element 
            foreach (var lockDto in _context.LockDto.Where(model => model.Id == EventId))
            {
                _context.LockDto.Remove(lockDto);
            }
            _context.SaveChanges();
        }

        public HashSet<RelationToOtherEventModel> Conditions
        {
            get
            {
                var dbset = _context.Conditions.Where(model => model.EventIdentificationModelId == EventId);
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
            set
            {
                foreach (var uri in _context.Conditions.Where(model => model.EventIdentificationModelId == EventId))
                {
                    _context.Conditions.Remove(uri);
                }

                foreach (var element in value)
                {
                    var uriToAdd = new ConditionUri()
                    {
                        UriString = element.Uri.AbsoluteUri,
                        EventId = element.EventID,
                        EventIdentificationModelId = EventId
                    };
                    _context.Conditions.Add(uriToAdd);
                }
                _context.SaveChanges();
            }
        }
        public HashSet<RelationToOtherEventModel> Responses 
        {
            get
            {
                var dbset = _context.Responses.Where(model => model.EventIdentificationModelId == EventId);
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
            set
            {
                foreach (var uri in _context.Responses.Where(model => model.EventIdentificationModelId == EventId))
                {
                    _context.Responses.Remove(uri);
                }

                foreach (var element in value)
                {
                    var uriToAdd = new ResponseUri()
                    {
                        UriString = element.Uri.AbsoluteUri,
                        EventId = element.EventID,
                        EventIdentificationModelId = EventId
                    };
                    _context.Responses.Add(uriToAdd);
                }
                _context.SaveChanges();
            }
        }
        public HashSet<RelationToOtherEventModel> Exclusions
        {
            get
            {
                var dbset = _context.Exclusions.Where(model => model.EventIdentificationModelId == EventId);
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
            set
            {
                foreach (var uri in _context.Exclusions.Where(model => model.EventIdentificationModelId == EventId))
                {
                    _context.Exclusions.Remove(uri);
                }

                foreach (var element in value)
                {
                    var uriToAdd = new ExclusionUri()
                    {
                        UriString = element.Uri.AbsoluteUri,
                        EventId = element.EventID,
                        EventIdentificationModelId = EventId
                    };
                    _context.Exclusions.Add(uriToAdd);
                }
                _context.SaveChanges();
            }
        }
        public HashSet<RelationToOtherEventModel> Inclusions
        {
            get
            {
                var dbset = _context.Inclusions.Where(model => model.EventIdentificationModelId == EventId);
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
            set
            {
                foreach (var uri in _context.Inclusions.Where(model => model.EventIdentificationModelId == EventId))
                {
                    _context.Inclusions.Remove(uri);
                }

                foreach (var element in value)
                {
                    var uriToAdd = new InclusionUri()
                    {
                        UriString = element.Uri.AbsoluteUri, 
                        EventId = element.EventID, 
                        EventIdentificationModelId = EventId
                    };
                    _context.Inclusions.Add(uriToAdd);
                }
                _context.SaveChanges();
            }
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
        private void EventIdentificationIsInALegalState()
        {
            var eventIdentification = _context.EventIdentification.Where(model => model.Id == EventId);
            // Check that there's currently only a single element in database
            if (eventIdentification.Count() > 1)
            {
                throw new ApplicationException(
                    "More than a single EventIdentification element in database-set in Event");
            }


            if (!eventIdentification.Any(model => model.Id == EventId))
            {
                throw new ApplicationException("EventIdentification was not initialized in Event." +
                                               "Count was zero");
            }
        }

        /// <summary>
        /// EventLockIsInALegalState makes two checks on LockDto-set,
        /// that when combined ensures that LockDto only has a single element. 
        /// </summary>
        private void EventLockIsInALegalState()
        {
            var lockDto = _context.LockDto.Where(model => model.Id == EventId);
            // Check that there's currently only a single element in database
            if (lockDto.Count() > 1)
            {
                throw new ApplicationException(
                    "More than a single Lock element in database-set in Event");
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