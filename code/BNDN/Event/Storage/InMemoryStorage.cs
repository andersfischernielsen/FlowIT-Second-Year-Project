using System;
using System.Collections.Generic;
using Event.Interfaces;

namespace Event.Models
{
    public class InMemoryStorage2 : IEventStorage
    {
        public InMemoryStorage2()
        {
            EventUris = new Dictionary<string, Uri>();
            EventIds = new Dictionary<Uri, string>();
            Conditions = new HashSet<Uri>();
            Responses = new HashSet<Uri>();
            Exclusions = new HashSet<Uri>();
            Inclusions = new HashSet<Uri>();
        }

        #region Properties
        public Uri OwnUri { get; set; }
        public string WorkflowId { get; set; }
        public string EventId { get; set; }
        public string Name { get; set; }
        public LockDto LockDto { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }
        public Dictionary<string, Uri> EventUris { get; set; }
        public string Role { get; set; }
        public Dictionary<Uri, string> EventIds { get; set; }
        public HashSet<Uri> Conditions { get; set; }
        public HashSet<Uri> Responses { get; set; }
        public HashSet<Uri> Exclusions { get; set; }
        public HashSet<Uri> Inclusions { get; set; }

        public ICollection<EventUriIdMapping> EventUriIdMappings
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region Methods For Removing/Storing Data
        public Uri GetUriFromId(string id)
        {
            return EventUris[id];
        }
        public string GetIdFromUri(Uri endPoint)
        {
            return EventIds[endPoint];
        }
        public void RemoveIdAndUri(string id)
        {
            EventIds.Remove(EventUris[id]);
            EventUris.Remove(id);
        }
        public bool IdExists(string id)
        {
            return EventUris.ContainsKey(id);
        }
        public void StoreIdAndUri(string id, Uri endPoint)
        {
            EventUris.Add(id, endPoint);
            EventIds.Add(endPoint, id);
        }
        #endregion

        public void Dispose()
        {
            // Don't do anything.
        }
    }
}