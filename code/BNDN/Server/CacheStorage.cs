using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Server
{
    public class CacheStorage : IServerStorage
    {
        private static CacheStorage _instance;
        private readonly Dictionary<int, List<EventAddressDto>> _cache = new Dictionary<int, List<EventAddressDto>>(); 

        private CacheStorage()
        {
        }

        public static CacheStorage GetStorage
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CacheStorage();
                }
                return _instance;
            }
        }

        public IList<WorkflowDto> GetAllWorkflows()
        {
            var l = _cache.Keys.ToList();
            var a = l.ConvertAll(x => new WorkflowDto() {Id = x, Name = "NoName with current implementation :("});
            return a;
        }

        public IList<EventAddressDto> GetEventsWithinWorkflow(int workflowId)
        {
            List<EventAddressDto> list;
            var b = _cache.TryGetValue(workflowId, out list);
            if (b)
            {
                return list;
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public void AddEventToWorkflow(int workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            if (_cache.ContainsKey(workflowToAttachToId))
            {
                List<EventAddressDto> list;
                var b = _cache.TryGetValue(workflowToAttachToId, out list);
                if (b)
                {
                    list.Add(eventToBeAddedDto);
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public void RemoveEventFromWorkflow(int workflowId, string eventId)
        {
            if (_cache.ContainsKey(workflowId))
            {
                List<EventAddressDto> list;
                var b = _cache.TryGetValue(workflowId, out list);
                if (b)
                {
                    list.RemoveAll(x => x.Id == eventId);
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            else
            {
                throw new NullReferenceException();
            }
        }
    }
}
