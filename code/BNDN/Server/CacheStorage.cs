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

        public IList<EventAddressDto> GetEventsWithinWorkflow(string workflowId)
        {
            List<EventAddressDto> list;
            var l = GetAllWorkflows();
            var w = l.Single(x => x.Id == workflowId);
            var b = _cache.TryGetValue(w, out list);
            if (b)
            {
                return list;
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        public void AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var l = GetAllWorkflows();
            var w = l.Single(x => x.Id == workflowToAttachToId);
            if (_cache.ContainsKey(w))
            {
                List<EventAddressDto> list;
                var b = _cache.TryGetValue(w, out list);
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

        public void RemoveEventFromWorkflow(string workflowId, string eventId)
        {
            var l = GetAllWorkflows();
            var w = l.Single(x => x.Id == workflowId);
            if (_cache.ContainsKey(w))
            {
                List<EventAddressDto> list;
                var b = _cache.TryGetValue(w, out list);
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
