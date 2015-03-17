using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;

namespace Event
{
    public class InMemoryStorage : IEventStorage
    {
        private static InMemoryStorage _inMemoryStorage;

        public string Id { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }

        private HashSet<IPEndPoint> _conditions;
        private HashSet<IPEndPoint> _responses;
        private HashSet<IPEndPoint> _exclusions;
        private HashSet<IPEndPoint> _inclusions;

        private readonly Dictionary<string, Tuple<bool, bool>> _preconditions = new Dictionary<string, Tuple<bool, bool>>();

        //TODO: IS THIS THE WAY TO GO?
        private Dictionary<string, IPEndPoint> EventEndPoints { get; set; }
        private Dictionary<IPEndPoint, string> EventIds { get; set; }

        private InMemoryStorage()
        {
            _conditions = new HashSet<IPEndPoint>();
            _responses = new HashSet<IPEndPoint>();
            _exclusions = new HashSet<IPEndPoint>();
            _inclusions = new HashSet<IPEndPoint>();
            EventEndPoints = new Dictionary<string, IPEndPoint>();
            EventIds = new Dictionary<IPEndPoint, string>();
        }

        public IEnumerable<IPEndPoint> Conditions
        {
            get { return _conditions; }
        }

        public IEnumerable<IPEndPoint> Responses
        {
            get { return _responses; }
        }

        public IEnumerable<IPEndPoint> Exclusions
        {
            get { return _exclusions; }
        }

        public IEnumerable<IPEndPoint> Inclusions
        {
            get { return _inclusions;  }
        }

        public void UpdateRules(string id, EventRuleDto rules)
        {
            var endPoint = EventEndPoints[id];
            if (endPoint == null)
            {
                throw new ArgumentException("Nonexistent id", id);
            }
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            // Condition
            if (rules.Condition)
            {
                _conditions.Add(endPoint);
            }
            else
            {
                _conditions.Remove(endPoint);
            }

            // Exclusion
            if (rules.Exclusion)
            {
                _exclusions.Add(endPoint);
            }
            else
            {
                _exclusions.Remove(endPoint);
            }

            // Inclusion
            if (rules.Inclusion)
            {
                _inclusions.Add(endPoint);
            }
            else
            {
                _inclusions.Remove(endPoint);
            }

            // Response
            if (rules.Response)
            {
                _responses.Add(endPoint);
            }
            else
            {
                _responses.Remove(endPoint);
            }
        }

        public Tuple<bool, bool> GetPrecondition(string id)
        {
            lock (_preconditions) { return _preconditions[id]; }
        }

        public IEnumerable<String> GetPreconditions()
        {
            lock (_preconditions)
            {
                return _preconditions.Keys;
            }
        }

        public void AddPrecondition(string key, Tuple<bool, bool> state)
        {
            lock (_preconditions)
            {
                _preconditions[key] = state;
            }
        }

        public Task<EventStateDto> EventStateDto
        {
            get
            {
                lock (_preconditions)
                {
                    return Task.Run(() => new EventStateDto
                    {
                        Executed = Executed,
                        Included = Included,
                        Pending = Pending,
                        Executable = _preconditions.Values.All(state => state.Item1 || state.Item2)
                    });
                }
            }
        }

        public IPEndPoint GetEndPointFromId(string id)
        {
            return EventEndPoints[id];
        }

        public string GetIdFromEndPoint(IPEndPoint endPoint)
        {
            return EventIds[endPoint];
        }

        public void RegisterIdWithEndPoint(string id, IPEndPoint endPoint)
        {
            EventEndPoints.Add(id, endPoint);
            EventIds.Add(endPoint, id);
        }

        public bool KnowsId(string id)
        {
            return EventEndPoints.ContainsKey(id);
        }

        public void RemoveIdAndEndPoint(string id)
        {
            EventIds.Remove(EventEndPoints[id]);
            EventEndPoints.Remove(id);
        }

        public Task<EventDto> EventDto
        {
            get
            {
                // Todo: Fix datastructure to remove ToList().
                return Task.Run(() => new EventDto
                {
                    Id = Id,
                    Pending = Pending,
                    Executed = Executed,
                    Included = Included,
                    Conditions = _conditions.ToList(),
                    Exclusions = _exclusions.ToList(),
                    Responses = _responses.ToList(),
                    Inclusions = _inclusions.ToList()
                });
            }
        }

        public static InMemoryStorage GetState()
        {
            if (_inMemoryStorage == null)
            {
                return _inMemoryStorage = new InMemoryStorage();
            }
            return _inMemoryStorage;
        }
    }
}