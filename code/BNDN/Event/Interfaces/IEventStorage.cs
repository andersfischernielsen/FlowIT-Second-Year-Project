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
        #endregion

        #region Locking
        LockDto LockDto { get; set; }
        void ClearLock();
        #endregion

        #region Rules
        HashSet<Uri> OldConditions { get; set; }
        HashSet<Uri> OldResponses { get; set; }
        HashSet<Uri> OldExclusions { get; set; }
        HashSet<Uri> OldInclusions { get; set; }

        HashSet<RelationToOtherEventModel> Conditions { get; set; }
        HashSet<RelationToOtherEventModel> Responses { get; set; }
        HashSet<RelationToOtherEventModel> Exclusions { get; set; }
        HashSet<RelationToOtherEventModel> Inclusions { get; set; }


        #endregion
    }
}