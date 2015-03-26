using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Event.Interfaces
{
    interface IEventLogic
    {
        Task UpdateRules(string id, EventRuleDto rules);

        #region DTO Methods
        Task<EventStateDto> EventStateDto { get; }
        Task<EventDto> EventDto { get; }
        #endregion
    }
}
