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

        public IList<WorkflowDto> GetAllWorkflows()
        {
            return _cache.Keys.ToList();
        }

        public IList<EventAddressDto> GetEventsWithinWorkflow(string workflowId)
        {
<<<<<<< HEAD
            List<EventAddressDto> list;
            _cache.TryGetValue(workflow, out list);
            return list;
=======
            HashSet<EventAddressDto> list;
            var l = GetAllWorkflows();
            var w = l.Single(x => x.Id == workflowId); //Throws exception if workflow is not found
            _cache.TryGetValue(w, out list);
            return list.ToList();
>>>>>>> c3f63f3fc0375ca5b44a4887fe9b798db060b5f2
        }

        private bool ContainsEvent(HashSet<EventAddressDto> workflow, EventAddressDto eventAddress)
        {
<<<<<<< HEAD
            List<EventAddressDto> list;
            var b = _cache.TryGetValue(workflow, out list);
            list.Add(eventToBeAddedDto);
=======
            if (workflow.Contains(eventAddress))
            {
                return false;
            }
            else
            {
                return true;
            }
>>>>>>> c3f63f3fc0375ca5b44a4887fe9b798db060b5f2
        }

        public void AddEventToWorkflow(string workflowToAttachToId, EventAddressDto eventToBeAddedDto)
        {
            var l = GetAllWorkflows();
            var w = l.Single(x => x.Id == workflowToAttachToId); //Throws exception if workflow is not found
            HashSet<EventAddressDto> list;
            _cache.TryGetValue(w, out list);
            if (ContainsEvent(list, eventToBeAddedDto))
            {
                throw new ArgumentException();
            }
            else
            {
                list.Add(eventToBeAddedDto);
            }
            
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
