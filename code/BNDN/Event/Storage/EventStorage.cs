using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Event.Interfaces;
using Event.Models;
using Event.Models.UriClasses;

namespace Event.Storage
{
    public class EventStorage : IEventStorage
    {
        private EventContext _context;

        public EventStorage()
        {
            _context = new EventContext();
        }

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
                // Check that there is not more than a single element in EventIdentification
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventIdentification");
                }

                var eventIdentificationPackage = _context.EventIdentification.FirstOrDefault();
                if (eventIdentificationPackage == null)
                {
                    return null;
                }
                return eventIdentificationPackage.WorkflowId;

            }
            set
            {
                // Check that there is no more than a single element in EventIdentification
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventIdentification");
                }
                if (!_context.EventIdentification.Any())
                {
                    throw new ApplicationException("EventIdentification was not initialized in Event");
                }

                _context.EventIdentification.Single().WorkflowId = value;
                _context.SaveChanges();
            }
        }

        public string EventId
        {
            get
            {
                // Check that there is no more than a single element in EventIdentification
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventIdentification");
                }
                if (!_context.EventIdentification.Any())
                {
                    throw new ApplicationException("EventIdentification was not initialized in Event");
                }

                var eventIdentificationPackage = _context.EventIdentification.FirstOrDefault();
                if (eventIdentificationPackage == null)
                {
                    return null;
                }
                return eventIdentificationPackage.EventId;

            }
            set
            {
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventIdentification");
                }
                if (!_context.EventIdentification.Any())
                {
                    throw new ApplicationException("EventIdentification was not initialized in Event");
                }


                _context.EventIdentification.Single().EventId = value;
                _context.SaveChanges();
            }
        }

        public string Name
        {
            get
            {
                // Check that there is no more than a single element in EventIdentification
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventIdentification");
                }
                if (!_context.EventIdentification.Any())
                {
                    throw new ApplicationException("EventIdentification was not initialized in Event");
                }
                return _context.EventIdentification.Single().Name;
            }
            set
            {
                // Check that there is no more than a single element in EventIdentification
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventIdentification");
                }
                if (!_context.EventIdentification.Any())
                {
                    throw new ApplicationException("EventIdentification was not initialized in Event");
                }

                _context.EventIdentification.Single().Name = value;
                _context.SaveChanges();

            }
        }

        public string Role
        {
            get
            {
                // Check that there is no more than a single element in EventIdentification
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventIdentification");
                }
                if (!_context.EventIdentification.Any())
                {
                    throw new ApplicationException("EventIdentification was not initialized in Event");
                }

                return _context.EventIdentification.Single().Role;
            }
            set
            {
                // Check that there is no more than a single element in EventIdentification
                if (_context.EventIdentification.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventIdentification");
                }
                if (!_context.EventIdentification.Any())
                {
                    throw new ApplicationException("EventIdentification was not initialized in Event");
                }

                _context.EventIdentification.Single().Role = value;
                _context.SaveChanges();
            }
        }

        public bool Executed
        {
            get
            {
                // Check that there is no more than a single element in EventState
                if (_context.EventState.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventState");
                }
                if (!_context.EventState.Any())
                {
                    throw new ApplicationException("EventState was not initialized in Event");
                }

                return _context.EventState.Single().Executed;
            }
            set
            {
                // Check that there is no more than a single element in EventState
                if (_context.EventState.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventState");
                }
                if (!_context.EventState.Any())
                {
                    throw new ApplicationException("EventStae was not initialized in Event");
                }

                _context.EventState.Single().Executed = value;
                _context.SaveChanges();
            }
        }

        public bool Included
        {
            get
            {
                // Check that there is no more than a single element in EventState
                if (_context.EventState.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventState");
                }
                if (!_context.EventState.Any())
                {
                    throw new ApplicationException("EventState was not initialized in Event");
                }
                return _context.EventState.Single().Included;
            }
            set
            {
                // Check that there is no more than a single element in EventState
                if (_context.EventState.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventState");
                }
                if (!_context.EventState.Any())
                {
                    throw new ApplicationException("EventState was not initialized in Event");
                }

                _context.EventState.Single().Included = value;
                _context.SaveChanges();
            }
        }

        public bool Pending
        {
            get
            {
                // Check that there is no more than a single element in EventState
                if (_context.EventState.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventState");
                }
                if (!_context.EventState.Any())
                {
                    throw new ApplicationException("EventState was not initialized in Event");
                }

                return _context.EventState.Single().Pending;
            }
            set
            {
                // Check that there is no more than a single element in EventState
                if (_context.EventState.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in EventState");
                }
                if (!_context.EventState.Any())
                {
                    throw new ApplicationException("EventState was not initialized in Event");
                }

                _context.EventState.Single().Pending = value;
                _context.SaveChanges();
            }
        }

        public LockDto LockDto
        {
            get
            {
                // Check that there is no more than a single element in LockDto
                if (_context.LockDto.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in LockDto");
                }
                //if (!_context.LockDto.Any())
                //{
                //    throw new ApplicationException("LockDto was not initialized in Event");
                //}

                var result = _context.LockDto.SingleOrDefault();

                return result;
            }
            set
            {
                // Check that there is no more than a single element in EventState
                if (_context.LockDto.Count() > 1)
                {
                    throw new ApplicationException("More than a single element in LockDto");
                }
                // Todo: I outcommented this, because it makes sense not to have a lockDto set at all times.
                //if (!_context.LockDto.Any())
                //{
                //    throw new ApplicationException("LockDto was not initialized in Event");
                //}

                foreach (var element in _context.LockDto)
                {
                    _context.LockDto.Remove(element);
                }


                _context.LockDto.Add(value);
                _context.SaveChanges();
            }
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
            var result = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Uri.Equals(endPoint.AbsoluteUri)).Result;
            if (result == null) return null;
            return result.Id;
        }

        public void RemoveIdAndUri(string id)
        {
            var toRemove = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
            if (toRemove == null) return;
            _context.EventUriIdMappings.Remove(toRemove);
            _context.SaveChanges();
        }

        // TODO: Discuss: Is this method also intended to be used for updating an existing entry? In that
        // TODO: case the current implementation is faulty...
        public void StoreIdAndUri(string id, Uri endPoint)
        {
            var eventUriIdMapping = new EventUriIdMapping() { Id = id, Uri = endPoint.AbsolutePath };
            _context.EventUriIdMappings.Add(eventUriIdMapping);

            _context.SaveChanges();

        }

        public bool IdExists(string id)
        {
            var result = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
            return result != null;

        }

        public void Dispose()
        {
            _context.Dispose();
            _context = null;
        }
    }
}