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
        private readonly Dictionary<WorkflowDto, HashSet<EventAddressDto>> _cache = new Dictionary<WorkflowDto, HashSet<EventAddressDto>>(); 

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

        public IEnumerable<WorkflowDto> GetAllWorkflows()
        {
            return _cache.Keys.ToList();
        }

        public IEnumerable<EventAddressDto> GetEventsOnWorkflow(WorkflowDto workflow)
        {
            return _cache[workflow];
        }

        public void AddEventToWorkflow(WorkflowDto workflow, EventAddressDto eventToBeAddedDto)
        {
            HashSet<EventAddressDto> list;
            _cache.TryGetValue(workflow, out list);
            if (ContainsEvent(list, eventToBeAddedDto))
            {
                throw new ArgumentException();
            }
            else
            {
                list.Add(eventToBeAddedDto);
            }
        }

        public void RemoveEventFromWorkflow(WorkflowDto workflow, string eventId)
        {
            HashSet<EventAddressDto> list;
            _cache.TryGetValue(workflow, out list);
            list.RemoveWhere(dto => dto.Id == eventId);
        }

        private bool ContainsEvent(HashSet<EventAddressDto> workflow, EventAddressDto eventAddress)
        {
            throw new NotImplementedException();
        }

        public void AddNewWorkflow(WorkflowDto workflow)
        {
            if (_cache.ContainsKey(workflow))
            {
                throw new ArgumentException();
            }
            else
            {
                _cache.Add(workflow, new HashSet<EventAddressDto>());
            }
        }
    }
}
