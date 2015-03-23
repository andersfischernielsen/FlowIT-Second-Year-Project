using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Models;

namespace Event.Interfaces
{
    public interface IEventStorage
    {
        EventDto EntireEventDto { get; set; } // todo maybe maybe not - see todo in InMemoryStorage.
        string WorkflowId { get; }
        string EventId { get; }

        #region Storage
        bool Executed { get; set; }
        bool Included { get; set; }
        bool Pending { get; set; }
        #endregion

        #region Rules
        Task<HashSet<Uri>> Conditions { get; set; }
        Task<HashSet<Uri>> Responses { get; set;  }
        Task<HashSet<Uri>> Exclusions { get; set; }
        Task<HashSet<Uri>> Inclusions { get; set; }

        Task<IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>> GetNotifyDtos();

        Task UpdateRules(string id, EventRuleDto rules);
        #endregion


        #region DtoMethods
        Task<EventStateDto> EventStateDto { get; }
        Task<EventDto> EventDto { get; }
        #endregion


        #region EndPointRegistering
        Task<Uri> GetUriFromId(string id);
        Task<string> GetIdFromUri(Uri endPoint);
        Task RegisterIdWithUri(string id, Uri endPoint);
        Task<bool> KnowsId(string id);
        Task RemoveIdAndUri(string id);
        #endregion
    }
}