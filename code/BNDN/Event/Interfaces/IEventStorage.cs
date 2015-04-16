using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventStorage : IDisposable
    {
        #region Ids

        Task<bool> Exists(string eventId);

        //For notifying server about this event. Is fetched when receiving EventDto on creation!
        Task<Uri> GetUri(string eventId);
        Task SetUri(string eventId, Uri value);

        Task<string> GetWorkflowId(string eventId);
        Task SetWorkflowId(string eventId, string value);

        Task<string> GetName(string eventId);
        Task SetName(string eventId, string value);

        Task<IEnumerable<string>> GetRoles(string eventId);
        Task SetRoles(string eventId, IEnumerable<string> value);
        #endregion

        Task InitializeNewEvent(InitialEventState initialEventState);
        Task DeleteEvent(string eventId);

        #region State
        Task<bool> GetExecuted(string eventId);
        Task SetExecuted(string eventId, bool value);

        Task<bool> GetIncluded(string eventId);
        Task SetIncluded(string eventId, bool value);

        Task<bool> GetPending(string eventId);
        Task SetPending(string eventId, bool value);
        #endregion

        #region Locking
        Task<LockDto> GetLockDto(string eventId);
        Task SetLockDto(string eventId, LockDto value);

        Task ClearLock(string eventId);
        #endregion

        #region Rules
        HashSet<RelationToOtherEventModel> GetConditions(string eventId);
        Task SetConditions(string eventId, HashSet<RelationToOtherEventModel> value);

        HashSet<RelationToOtherEventModel> GetResponses(string eventId);
        Task SetResponses(string eventId, HashSet<RelationToOtherEventModel> value);

        HashSet<RelationToOtherEventModel> GetExclusions(string eventId);
        Task SetExclusions(string eventId, HashSet<RelationToOtherEventModel> value);

        HashSet<RelationToOtherEventModel> GetInclusions(string eventId);
        Task SetInclusions(string eventId, HashSet<RelationToOtherEventModel> value);
        #endregion
    }
}