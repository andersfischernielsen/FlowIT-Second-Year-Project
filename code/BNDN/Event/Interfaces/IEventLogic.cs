using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventLogic : IDisposable
    {
        #region State 
        bool Executed { get; set; }
        bool Included { get; set; }
        bool Pending { get; set; }
        IEnumerable<string> Roles { get; set; }

        string EventId { get; set; }
        #endregion

        #region Locking
        LockDto LockDto { get; set; }
        void UnlockEvent();
        #endregion

        #region Rules
        Task<bool> IsExecutable();
        #endregion

        #region DTO Creation
        Task<EventStateDto> GetEventStateDto();
        EventDto GetEventDto();
        IEnumerable<RelationToOtherEventModel> RelationsToLock { get; }

        #endregion

        bool CallerIsAllowedToOperate(string lockOwnerId);
        bool IsLocked();

        // TODO: I (Morten) may have bloated this interface; should we refactor
        Task InitializeEvent(EventDto eventDto, Uri ownUri);
        Task DeleteEvent();

        Task Execute();

        bool EventIdExists();

        bool ProvidedRolesHasMatchWithEventRoles(IEnumerable<string> providedRoles);
    }
}