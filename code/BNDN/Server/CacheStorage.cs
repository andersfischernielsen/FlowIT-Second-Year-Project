using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Server
{
    public class CacheStorage : IStorage
    {
        private static CacheStorage _instance;
        private readonly Dictionary<WorkflowDto, List<EventAddressDto>> _cache = new Dictionary<WorkflowDto, List<EventAddressDto>>(); 

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

        public IList<WorkflowDto> GetWorkflows()
        {
            return _cache.Keys.ToList();
        }

        public IList<EventAddressDto> GetEvents(WorkflowDto workflow)
        {
            List<EventAddressDto> list;
            var b =_cache.TryGetValue(workflow, out list);
            if (b)
            {
                return list;
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public void AddEventToWorkflow(WorkflowDto workflow, EventAddressDto eventDto)
        {
            if (_cache.ContainsKey(workflow))
            {
                List<EventAddressDto> list;
                var b = _cache.TryGetValue(workflow, out list);
                if (b)
                {
                    list.Add(eventDto);
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

        public void RemoveEventFromWorkflow(WorkflowDto workflow, EventAddressDto eventDto)
        {
            if (_cache.ContainsKey(workflow))
            {
                List<EventAddressDto> list;
                var b = _cache.TryGetValue(workflow, out list);
                if (b)
                {
                    list.Remove(eventDto);
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
