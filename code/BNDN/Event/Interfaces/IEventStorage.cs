using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventStorage
    {
        #region Ids
        Uri OwnUri { get; set; } //For notifying server about this event. Is fetched when receiving EventDto on creation!
        string WorkflowId { get; }
        string EventId { get; }
        #endregion

        #region State
        bool Executed { get; set; }
        bool Included { get; set; }
        bool Pending { get; set; }
        #endregion

        #region Rules
        HashSet<Uri> Conditions { get; set; }
        HashSet<Uri> Responses { get; set; }
        HashSet<Uri> Exclusions { get; set; }
        HashSet<Uri> Inclusions { get; set; }
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