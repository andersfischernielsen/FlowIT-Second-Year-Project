using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventStorage : IDisposable
    {
        #region Ids
        Uri OwnUri { get; set; } //For notifying server about this event. Is fetched when receiving EventDto on creation!
        string WorkflowId { get; set; }
        string EventId { get; set; }
        string Name { get; set; }
        string Role { get; set; }
        #endregion

        #region State
        bool Executed { get; set; }
        bool Included { get; set; }
        bool Pending { get; set; }
        LockDto LockDto { get; set; }
        #endregion

        #region Rules
        HashSet<Uri> Conditions { get; set; }
        HashSet<Uri> Responses { get; set; }
        HashSet<Uri> Exclusions { get; set; }
        HashSet<Uri> Inclusions { get; set; }
        ICollection<EventUriIdMapping> EventUriIdMappings { get; set; }


        #endregion

        #region Id and Uri Handling
        Uri GetUriFromId(string id);
        string GetIdFromUri(Uri endPoint);
        void RemoveIdAndUri(string id);
        void StoreIdAndUri(string id, Uri endPoint);
        bool IdExists(string id);
        #endregion
    }
}