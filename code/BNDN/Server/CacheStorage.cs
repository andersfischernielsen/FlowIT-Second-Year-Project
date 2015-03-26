using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Common;

namespace Server
{
    public class CacheStorage : IServerStorage
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

        public IList<WorkflowDto> GetAllWorkflows()
        {
            return _cache.Keys.ToList();
        }

        public IList<EventAddressDto> GetEventsOnWorkflow(WorkflowDto workflow)
        {
            List<EventAddressDto> list;
            _cache.TryGetValue(workflow, out list);
            return list;
        }

        public void AddEventToWorkflow(WorkflowDto workflow, EventAddressDto eventToBeAddedDto)
        {
            List<EventAddressDto> list;
            var b = _cache.TryGetValue(workflow, out list);
            list.Add(eventToBeAddedDto);
        }

        public void RemoveEventFromWorkflow(WorkflowDto workflow, string eventId)
        {
            if (_cache.ContainsKey(workflow))
            {
                List<EventAddressDto> list;
                var b = _cache.TryGetValue(workflow, out list);
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

        public void AddNewWorkflow(WorkflowDto workflow)
        {
            if (_cache.ContainsKey(workflow))
            {
                throw new ArgumentException();
            }
            else
            {
                _cache.Add(workflow, new List<EventAddressDto>());
            }
        }
    }
}
