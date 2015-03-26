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
            throw new NotImplementedException();
        }

        public IEnumerable<EventAddressDto> GetEventsWithinWorkflow(string workflowId)
        {
            
        }

        private bool ContainsEvent(HashSet<EventAddressDto> workflow, EventAddressDto eventAddress)
        {
            throw new NotImplementedException();
        }

        public void RemoveEventFromWorkflow(string workflowId, string eventId)
        {
            var l = GetAllWorkflows();
            var w = l.Single(x => x.Id == workflowId); // Throws exception if workflow is not found
            HashSet<EventAddressDto> list;
            _cache.TryGetValue(w, out list);
            list.RemoveWhere(x => x.Id == eventId);
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
