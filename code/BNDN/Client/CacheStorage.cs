using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Client
{
    public class CacheStorage : IStorage
    {
        private static CacheStorage _instance;
        private readonly Dictionary<string, List<EventAddressDto>> _cache = new Dictionary<string, List<EventAddressDto>>(); 

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
            return _cache.Keys.Select(k => new WorkflowDto {Name = k}).ToList();
        }

        public IList<EventAddressDto> GetEvents(string workflow)
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

        public void AddEventToWorkflow(string workflow, EventAddressDto eventDto)
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

        public void RemoveEventFromWorkflow(string workflow, EventAddressDto eventDto)
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
