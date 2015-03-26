using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    interface IEventLogic
    {
        #region State
        LockDto LockDto { get; set; }  
        bool Executed { get; set; }
        bool Included { get; set; }
        bool Pending { get; set; }
        #endregion

        #region Rules
        Task<bool> IsExecutable();
        Task UpdateRules(string id, EventRuleDto rules);
        #endregion

        #region DTO Creation
        Task<EventStateDto> EventStateDto { get; }
        Task<EventDto> EventDto { get; }
        Task<IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>> GetNotifyDtos();

        Task AddNotifyDto<T>(IDictionary<Uri, List<NotifyDto>> dictionary, Uri uri, Func<string, T> creator)
            where T : NotifyDto;
        #endregion

        #region URI Handling
        Task RegisterIdWithUri(string id, Uri endPoint);
        Task<bool> KnowsId(string id);
        Task RemoveIdAndUri(string id);
        #endregion

        Task ResetState();
        bool IsAllowedToOperate(EventAddressDto eventAddressDto);

        Task InitializeEvent(EventDto eventDto, Uri ownUri);
        Task UpdateEvent(EventDto eventDto, Uri ownUri);
        Task DeleteEvent();
    }
}