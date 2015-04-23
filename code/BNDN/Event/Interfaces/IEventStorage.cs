using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventStorage : IDisposable, IEventHistoryStorage
    {
        #region Ids

        Task<bool> Exists(string workflowId, string eventId);

        //For notifying server about this event. Is fetched when receiving EventDto on creation!
        Task<Uri> GetUri(string workflowId, string eventId);
        Task SetUri(string workflowId, string eventId, Uri value);

        Task<string> GetName(string workflowId, string eventId);
        Task SetName(string workflowId, string eventId, string value);

        Task<IEnumerable<string>> GetRoles(string workflowId, string eventId);
        Task SetRoles(string workflowId, string eventId, IEnumerable<string> value);
        #endregion

        Task InitializeNewEvent(EventModel eventModel);
        Task DeleteEvent(string workflowId, string eventId);

        #region State
        Task<bool> GetExecuted(string workflowId, string eventId);
        Task SetExecuted(string workflowId, string eventId, bool value);

        Task<bool> GetIncluded(string workflowId, string eventId);
        Task SetIncluded(string workflowId, string eventId, bool value);

        Task<bool> GetPending(string workflowId, string eventId);
        Task SetPending(string workflowId, string eventId, bool value);
        #endregion

        #region Locking
        Task<LockDto> GetLockDto(string workflowId, string eventId);
        Task SetLock(string workflowId, string eventId, string lockOwner);

        Task ClearLock(string workflowId, string eventId);
        #endregion

        #region Rules
        Task<HashSet<RelationToOtherEventModel>> GetConditions(string workflowId, string eventId);
        Task SetConditions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value);

        Task<HashSet<RelationToOtherEventModel>> GetResponses(string workflowId, string eventId);
        Task SetResponses(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value);

        Task<HashSet<RelationToOtherEventModel>> GetExclusions(string workflowId, string eventId);
        Task SetExclusions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value);

        Task<HashSet<RelationToOtherEventModel>> GetInclusions(string workflowId, string eventId);
        Task SetInclusions(string workflowId, string eventId, HashSet<RelationToOtherEventModel> value);
        #endregion
    }
}