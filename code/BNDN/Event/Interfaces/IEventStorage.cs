using System;
using System.Collections.Generic;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventStorage : IDisposable
    {
        #region Ids
        //For notifying server about this event. Is fetched when receiving EventDto on creation!
        Uri GetUri(string eventId);
        void SetUri(string eventId, Uri value);

        string GetWorkflowId(string eventId);
        void SetWorkflowId(string eventId, string value);

        string GetName(string eventId);
        void SetName(string eventId, string value);

        IEnumerable<string> GetRoles(string eventId);
        void SetRoles(string eventId, IEnumerable<string> value);
        #endregion

        void InitializeNewEvent(InitialEventState initialEventState);
        void DeleteEvent(string eventId);

        #region State
        bool GetExecuted(string eventId);
        void SetExecuted(string eventId, bool value);

        bool GetIncluded(string eventId);
        void SetIncluded(string eventId, bool value);

        bool GetPending(string eventId);
        void SetPending(string eventId, bool value);
        #endregion

        #region Locking
        LockDto GetLockDto(string eventId);
        void SetLockDto(string eventId, LockDto value);

        void ClearLock(string eventId);
        #endregion

        #region Rules
        HashSet<RelationToOtherEventModel> GetConditions(string eventId);
        void SetConditions(string eventId, HashSet<RelationToOtherEventModel> value);

        HashSet<RelationToOtherEventModel> GetResponses(string eventId);
        void SetResponses(string eventId, HashSet<RelationToOtherEventModel> value);

        HashSet<RelationToOtherEventModel> GetExclusions(string eventId);
        void SetExclusions(string eventId, HashSet<RelationToOtherEventModel> value);

        HashSet<RelationToOtherEventModel> GetInclusions(string eventId);
        void SetInclusions(string eventId, HashSet<RelationToOtherEventModel> value);
        #endregion
    }
}