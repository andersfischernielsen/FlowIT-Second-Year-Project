using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common;

namespace Event
{
    public interface IEventStorage
    {
        string Id { get; set; }

        #region InMemoryStorage
        bool Executed { get; set; }
        bool Included { get; set; }
        bool Pending { get; set; } 
        #endregion

        #region Rules
        IEnumerable<IPEndPoint> Conditions { get; } 
        IEnumerable<IPEndPoint> Responses { get; }
        IEnumerable<IPEndPoint> Exclusions { get; }
        IEnumerable<IPEndPoint> Inclusions { get; }

        void UpdateRules(string id, EventRuleDto rules);
        #endregion


        #region DtoMethods
        Task<EventStateDto> EventStateDto { get; }
        Task<EventDto> EventDto { get; } 
        #endregion


        #region Preconditions
        Tuple<bool, bool> GetPrecondition(string id);
        IEnumerable<String> GetPreconditions();
        void AddPrecondition(string key, Tuple<bool, bool> state); 
        #endregion


        #region EndPointRegistering
		IPEndPoint GetEndPointFromId(string id);
        string GetIdFromEndPoint(IPEndPoint endPoint);
        void RegisterIdWithEndPoint(string id, IPEndPoint endPoint);
        bool KnowsId(string id);
	    void RemoveIdAndEndPoint(string id);
        #endregion
    }
}