using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        #endregion

        #region Locking
        LockDto LockDto { get; set; }
        void UnlockEvent();
        #endregion

        #region Rules
        Task<bool> IsExecutable();
        Task UpdateRules(string id, EventRuleDto rules);
        #endregion

        #region DTO Creation
        Task<EventStateDto> EventStateDto { get; }
        Task<EventDto> EventDto { get; }
        #endregion

        bool CallerIsAllowedToOperate(string lockOwnerId);
        bool IsLocked();

        // TODO: I (Morten) may have bloated this interface; should we refactor
        Task InitializeEvent(EventDto eventDto, Uri ownUri);
        Task UpdateEvent(EventDto eventDto, Uri ownUri);
        Task DeleteEvent();

        Task Execute();

        bool EventIdExists();

    }
}