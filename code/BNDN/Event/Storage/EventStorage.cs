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

        // TODO: Discuss: Do we need to (dependency)-inject the (db)context for testing purposes, instead?
        // TODO: If not, if constructor is parameterless, could / should we make EventStorage static then?
        public EventStorage()
        {

        }


        #region Properties
        public Uri OwnUri
        {
            get
            {
                using (var context = new EventContext())
                {
                    // We should only have one of these objects in the database
                    if (context.EventIdentification.Count() > 1)
                    {
                        throw new ApplicationException("More than one EventIdentification object in database");
                    }

                    var eventIdPackage = context.EventIdentification.SingleOrDefault();
                    if (eventIdPackage == null)
                    {
                        return null;
                    }

                    return new Uri(eventIdPackage.OwnUri);                    
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    // Check that there's currently only a single element in database
                    if (context.EventIdentification.Count() > 1)
                    {
                        throw new ApplicationException(
                            "More than a single EventIdentification element in database in Event");
                    }

                    if (context.EventIdentification.Count() == 0)
                    {
                        throw new ApplicationException("EventIdentification was not initialized in Event");
                    }

                    // Add replacing value
                    context.EventIdentification.Single().OwnUri = value.AbsoluteUri;
                    context.SaveChangesAsync();             
                }

            }
        }
        
        public string WorkflowId
        {
            get
            {
                using (var context = new EventContext())
                {
                    EventIdentificationIsInALegalState(context);

                    var eventIdentificationPackage = context.EventIdentification.FirstOrDefault();
                    if (eventIdentificationPackage == null)
                    {
                        return null;
                    }
                    return eventIdentificationPackage.WorkflowId;
                }

            }
            set
            {
                using (var context = new EventContext())
                {
                    EventIdentificationIsInALegalState(context);

                    context.EventIdentification.Single().WorkflowId = value;
                    context.SaveChangesAsync();                    
                }
            }
        }

        public string EventId
        {
            get
            {
                using (var context = new EventContext())
                {
                    EventIdentificationIsInALegalState(context);

                    var eventIdentificationPackage = context.EventIdentification.FirstOrDefault();
                    if (eventIdentificationPackage == null)
                    {
                        return null;
                    }
                    return eventIdentificationPackage.EventId;
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    EventIdentificationIsInALegalState(context);

                    context.EventIdentification.Single().EventId = value;
                    context.SaveChangesAsync();                    
                }
            }
        }

        public string Name
        {
            get
            {
                using (var context = new EventContext())
                {
                    EventIdentificationIsInALegalState(context);
                    return context.EventIdentification.Single().Name;    
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    EventIdentificationIsInALegalState(context);

                    context.EventIdentification.Single().Name = value;
                    context.SaveChangesAsync();    
                }
            }
        }

        public string Role
        {
            get
            {
                using (var context = new EventContext())
                {
                    EventIdentificationIsInALegalState(context);

                    return context.EventIdentification.Single().Role;    
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    EventIdentificationIsInALegalState(context);

                    context.EventIdentification.Single().Role = value;
                    context.SaveChangesAsync();    
                }
            }
        }

        public bool Executed
        {
            get
            {
                using (var context = new EventContext())
                {
                    EventStateIsInALegalState(context);
             
                    return context.EventState.Single().Executed;    
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    EventStateIsInALegalState(context);

                    context.EventState.Single().Executed = value;
                    context.SaveChangesAsync();    
                }
            }
        }

        public bool Included
        {
            get
            {
                using (var context = new EventContext())
                {
                    EventStateIsInALegalState(context);

                    return context.EventState.Single().Included;    
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    EventStateIsInALegalState(context);

                    context.EventState.Single().Included = value;
                    context.SaveChangesAsync();    
                }
            }
        }

        public bool Pending
        {
            get
            {
                using (var context = new EventContext())
                {
                    EventStateIsInALegalState(context);

                    return context.EventState.Single().Pending;    
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    EventStateIsInALegalState(context);

                    context.EventState.Single().Pending = value;
                    context.SaveChangesAsync();    
                }
            }
        }

        public LockDto LockDto
        {
            get
            {
                using (var context = new EventContext())
                {
                    // Intended there is no checks here, as LockDto is not part of / held in EventState or EventIdentification
                    var result = context.LockDto.SingleOrDefault();
                    
                    return result;
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    // Check that there is no more than a single element in LockDto set
                    if (context.LockDto.Count() > 1)
                    {
                        throw new ApplicationException("More than a single element in LockDto");
                    }

                    // Remove current LockDto
                    foreach (var element in context.LockDto)
                    {
                        context.LockDto.Remove(element);
                    }


                    // TODO: Discuss: As of now, we cannot tell whether the caller intended to clear the LockDto (i.e. unlock this Event)
                    // TODO: or if he accidentally wanted to set the lock, but provided a null-LockDto. Suggestion: Provide a ClearLock() method instead. 
                    // TODO: Recap: This setter serves two purposes; setting the lock and clearing it! More elegant with a ClearLock() method (according to me, Morten)
                    context.LockDto.Add(value);
                    context.SaveChangesAsync();    
                }
            }
        }

        public HashSet<Uri> Conditions
        {
            get
            {   
                using (var context = new EventContext())
                {
                    // No need to do zero or ">1" count check here; that is perfectly legal
                    var dbset = context.Conditions;
                    var hashSet = new HashSet<Uri>();
                    
                    foreach (var element in dbset)
                    {
                        hashSet.Add(new Uri(element.UriString));
                    }

                    return hashSet;    
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    // Reset current list
                    foreach (var uri in context.Conditions)
                    {
                        context.Conditions.Remove(uri);
                    }

                    // Add replacing values
                    foreach (var element in value)
                    {
                        var uriToAdd = new ConditionUri() { UriString = element.AbsoluteUri };
                        context.Conditions.Add(uriToAdd);
                    }

                    context.SaveChangesAsync();    
                }
                
            }
        }

        public HashSet<Uri> Responses
        {
            get
            {
                using (var context = new EventContext())
                {
                    var dbset = context.Responses;
                    var hashSet = new HashSet<Uri>();
                 
                    foreach (var element in dbset)
                    {
                        hashSet.Add(new Uri(element.UriString));
                    }

                    return hashSet;   
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    // Remove current content 
                    foreach (var uri in context.Responses)
                    {
                        context.Responses.Remove(uri);
                    }

                    // Add replacing content
                    foreach (var element in value)
                    {
                        var uriToAdd = new ResponseUri() { UriString = element.AbsoluteUri };
                        context.Responses.Add(uriToAdd);
                    }

                    context.SaveChangesAsync();
                }
            }
        }

        public HashSet<Uri> Exclusions
        {
            get
            {
                using (var context = new EventContext())
                {
                    var dbset = context.Exclusions;
                    var hashSet = new HashSet<Uri>();
                 
                    foreach (var element in dbset)
                    {
                        hashSet.Add(new Uri(element.UriString));
                    }

                    return hashSet;
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    // Remove current content
                    foreach (var uri in context.Exclusions)
                    {
                        context.Exclusions.Remove(uri);
                    }

                    // Add replacing values
                    foreach (var element in value)
                    {
                        var uriToAdd = new ExclusionUri() { UriString = element.AbsoluteUri };
                        context.Exclusions.Add(uriToAdd);
                    }

                    context.SaveChangesAsync();
                }
            }
        }

        public HashSet<Uri> Inclusions
        {
            get
            {
                using (var context = new EventContext())
                {
                    var dbset = context.Inclusions;
                    var hashSet = new HashSet<Uri>();

                    foreach (var element in dbset)
                    {
                        hashSet.Add(new Uri(element.UriString));
                    }

                    return hashSet;   
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    foreach (var uri in context.Inclusions)
                    {
                        context.Inclusions.Remove(uri);
                    }

                    foreach (var element in value)
                    {
                        var uriToAdd = new InclusionUri() { UriString = element.AbsoluteUri };
                        context.Inclusions.Add(uriToAdd);
                    }
                    context.SaveChangesAsync();
                }
            }
        }

        public ICollection<EventUriIdMapping> EventUriIdMappings
        {
            get
            {
                using (var context = new EventContext())
                {
                    return context.EventUriIdMappings.ToList();                    
                }
            }
            set
            {
                using (var context = new EventContext())
                {
                    // Remove current entries
                    foreach (var element in context.EventUriIdMappings)
                    {
                        context.EventUriIdMappings.Remove(element);
                    }

                    // Add replacing entries
                    foreach (var element in value)
                    {
                        context.EventUriIdMappings.Add(element);
                    }
                    context.SaveChangesAsync();   
                }
            }
        }
        #endregion

        #region Public methods
        public Uri GetUriFromId(string id)
        {
            using (var context = new EventContext())
            {
                // TODO: Discuss: Use Task instead and await here?: Update IEventStorage to reflect
                var uri = context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
                if (uri == null) return null;
                return new Uri(uri.Uri);        
            }
        }

        /// <summary>
        /// Given an URI-object (representing another Event's URI) this method returns the related id.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public string GetIdFromUri(Uri endPoint)
        {
            using (var context = new EventContext())
            {
                var result = context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Uri.Equals(endPoint.AbsoluteUri)).Result;
                if (result == null) return null;
                return result.Id;                    
            }
        }

        public void RemoveIdAndUri(string id)
        {
            using (var context = new EventContext())
             {
                var toRemove = context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
                if (toRemove == null) return;
                context.EventUriIdMappings.Remove(toRemove);
                context.SaveChangesAsync();
             }
        }

        // TODO: Discuss: Is this method also intended to be used for updating an existing entry? In that
        // TODO: case the current implementation is faulty...
        public void StoreIdAndUri(string id, Uri endPoint)
        {
            using (var context = new EventContext())
            {
                var eventUriIdMapping = new EventUriIdMapping() { Id = id, Uri = endPoint.AbsolutePath };
                context.EventUriIdMappings.Add(eventUriIdMapping);

                context.SaveChangesAsync();   
            }
           
        }

        public bool IdExists(string id)
        {
            using (var context = new EventContext())
            {
                var result = context.EventUriIdMappings.FirstOrDefaultAsync(x => x.Id == id).Result;
                return result != null;        
            }
        }

        #endregion

        #region Private Methods

        private void EventIdentificationIsInALegalState(EventContext context)
        {
            // Check that there's currently only a single element in database
            if (context.EventIdentification.Count() > 1)
            {
                throw new ApplicationException(
                    "More than a single EventIdentification element in database-set in Event");
            }

            if (context.EventIdentification.Count() == 0)
            {
                throw new ApplicationException("EventIdentification was not initialized in Event." +
                                               "Count was zero");
            }                

        }

        private void EventStateIsInALegalState(EventContext context)
        {
            // Check that there is no more than a single element in EventState
            if (context.EventState.Count() > 1)
            {
                throw new ApplicationException("More than a single element in EventState set");
            }
            if (context.EventState.Count() == 0)
            {
                throw new ApplicationException("EventState was not initialized in Event");
            }                
        }
        #endregion
    }
}