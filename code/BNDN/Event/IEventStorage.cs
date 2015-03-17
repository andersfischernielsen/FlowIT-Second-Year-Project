using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common;

namespace Event
{
    public interface IEventStorage
    {
        string Id { get; }

        #region InMemoryStorage
        bool Executed { get; set; }
        bool Included { get; set; }
        bool Pending { get; set; } 
        #endregion

        #region Rules
        Task<IEnumerable<IPEndPoint>> Conditions { get; } 
        Task<IEnumerable<IPEndPoint>> Responses { get; }
        Task<IEnumerable<IPEndPoint>> Exclusions { get; }
        Task<IEnumerable<IPEndPoint>> Inclusions { get; }

        Task UpdateRules(string id, EventRuleDto rules);
        #endregion


        #region DtoMethods
        Task<EventStateDto> EventStateDto { get; }
        Task<EventDto> EventDto { get; } 
        #endregion


        #region Preconditions
        Task<Tuple<bool, bool>> GetPrecondition(string id);
        Task<IEnumerable<string>> GetPreconditions();
        Task AddPrecondition(string key, Tuple<bool, bool> state); 
        #endregion


        #region EndPointRegistering
		Task<IPEndPoint> GetEndPointFromId(string id);
        Task<string> GetIdFromEndPoint(IPEndPoint endPoint);
        Task RegisterIdWithEndPoint(string id, IPEndPoint endPoint);
        Task<bool> KnowsId(string id);
	    Task RemoveIdAndEndPoint(string id);
        #endregion
    }
}