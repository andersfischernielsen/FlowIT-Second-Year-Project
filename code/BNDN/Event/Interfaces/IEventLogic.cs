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
        bool Executed { get; set; }
        bool Included { get; set; }
        bool Pending { get; set; }
        #endregion

        #region Rules
        Task UpdateRules(string id, EventRuleDto rules);
        #endregion

        #region DTO Creation
        Task<EventStateDto> EventStateDto { get; }
        Task<EventDto> EventDto { get; }
        Task<IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>> GetNotifyDtos();

        Task AddNotifyDto<T>(IDictionary<Uri, List<NotifyDto>> dictionary, Uri uri, Func<string, T> creator)
            where T : NotifyDto;
        #endregion
    }
}
