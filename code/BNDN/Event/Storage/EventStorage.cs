using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
                var uriString = _context.EventIdentification.Single().OwnUri;
                return new Uri(uriString);
            }
            set
            {
                _context.EventIdentification.Single().OwnUri = value.AbsoluteUri;
                _context.SaveChangesAsync();
            }
        }
        
        public string WorkflowId
        {
            get { return _context.EventIdentification.Single().WorkflowId; }
            set
            {
                _context.EventIdentification.Single().WorkflowId = value;
                _context.SaveChangesAsync();
            }
        }

        public string EventId
        {
            get { return _context.EventIdentification.Single().EventId; }
            set
            {
                _context.EventIdentification.Single().EventId = value;
                _context.SaveChangesAsync();
            }
        }

        public string Name
        {
            get { return _context.EventIdentification.Single().Name; }
            set
            {
                _context.EventIdentification.Single().Name = value;
                _context.SaveChangesAsync();
            }
        }

        public string Role
        {
            get { return _context.EventIdentification.Single().Role; }
            set
            {
                _context.EventIdentification.Single().Role = value;
                _context.SaveChangesAsync();
            }
        }

        public bool Executed
        {
            get { return _context.EventState.Single().Executed; }
            set
            {
                _context.EventState.Single().Executed = value;
                _context.SaveChangesAsync();
            }
        }

        public bool Included
        {
            get { return _context.EventState.Single().Included; }
            set
            {
                _context.EventState.Single().Included = value;
                _context.SaveChangesAsync();
            }
        }

        public bool Pending
        {
            get { return _context.EventState.Single().Pending; }
            set
            {
                _context.EventState.Single().Pending = value;
                _context.SaveChangesAsync();
            }
        }

        public LockDto LockDto
        {
            get { return _context.EventState.Single().LockDto; }
            set
            {
                _context.EventState.Single().LockDto = value;
                _context.SaveChangesAsync();
            }
        }

        public HashSet<Uri> Conditions
        {
            get
            {   
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
                foreach (var uri in _context.Conditions)
                {
                    _context.Conditions.Remove(uri);
                }

                foreach (var element in value)
                {
                    var uriToAdd = new ConditionUri(){UriString = element.AbsoluteUri};
                    _context.Conditions.Add(uriToAdd);

                }

                _context.SaveChangesAsync();
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
                foreach (var uri in _context.Responses)
                {
                    _context.Responses.Remove(uri);
                }

                foreach (var element in value)
                {
                    var uriToAdd = new ResponseUri() { UriString = element.AbsoluteUri };
                    _context.Responses.Add(uriToAdd);

                }

                _context.SaveChangesAsync();
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
                foreach (var uri in _context.Exclusions)
                {
                    _context.Exclusions.Remove(uri);
                }

                foreach (var element in value)
                {
                    var uriToAdd = new ExclusionUri() { UriString = element.AbsoluteUri };
                    _context.Exclusions.Add(uriToAdd);

                }

                _context.SaveChangesAsync();
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

                _context.SaveChangesAsync();
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
                foreach (var element in _context.EventUriIdMappings)
                {
                    _context.EventUriIdMappings.Remove(element);
                }

                foreach (var element in value)
                {
                    _context.EventUriIdMappings.Add(element);
                }

                _context.SaveChangesAsync();
            }
        }
                
            
        

        public Uri GetUriFromId(string id)
        {
            // TODO: Use Task instead and await: Update IEventStorge
            var uri = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
            if (uri == null) return null;
            return uri.Uri;
        }

        public string GetIdFromUri(Uri endPoint)
        {
            var id = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Uri.Equals(endPoint)).Result;
            return id.Id;
        }

        public void RemoveIdAndUri(string id)
        {
            var toRemove = _context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
            _context.EventUriIdMappings.Remove(toRemove);

            _context.SaveChangesAsync();
        }

        public void StoreIdAndUri(string id, Uri endPoint)
        {
            var eventUriIdMapping = new EventUriIdMapping() {Id = id, Uri = endPoint};
            _context.EventUriIdMappings.Add(eventUriIdMapping);

            _context.SaveChangesAsync();
        }

        public bool IdExists(string id)
        {
            var result =_context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id==id).Result;
            return result != null;
        }
    }
}