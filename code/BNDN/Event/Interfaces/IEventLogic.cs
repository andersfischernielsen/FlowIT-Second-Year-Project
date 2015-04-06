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
        string Role { get; set; }
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
        Task<IEnumerable<Uri>> GetNotifyDtos();

        Task AddNotifyDto<T>(IDictionary<Uri, List<NotifyDto>> dictionary, Uri uri, Func<string, T> creator)
            where T : NotifyDto;
        #endregion

        Task ResetState();
        bool CallerIsAllowedToOperate(EventAddressDto eventAddressDto);
        bool IsLocked();

        // TODO: I (Morten) may have bloated this interface; should we refactor
        Task InitializeEvent(EventDto eventDto, Uri ownUri);
        Task UpdateEvent(EventDto eventDto, Uri ownUri);
        Task DeleteEvent();

        Task Execute();

    }
}