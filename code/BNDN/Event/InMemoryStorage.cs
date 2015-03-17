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

        private readonly HashSet<IPEndPoint> _conditions;
        private readonly HashSet<IPEndPoint> _responses;
        private readonly HashSet<IPEndPoint> _exclusions;
        private readonly HashSet<IPEndPoint> _inclusions;

        private readonly Dictionary<string, Tuple<bool, bool>> _preconditions = new Dictionary<string, Tuple<bool, bool>>();

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

        public Task<IEnumerable<IPEndPoint>> Conditions
        {
            get { return Task.Run(() => _conditions.AsEnumerable()); }
        }

        public Task<IEnumerable<IPEndPoint>> Responses
        {
            get { return Task.Run(() => _responses.AsEnumerable()); }
        }

        public Task<IEnumerable<IPEndPoint>> Exclusions
        {
            get { return Task.Run(() => _exclusions.AsEnumerable()); }
        }

        public Task<IEnumerable<IPEndPoint>> Inclusions
        {
            get { return Task.Run(() => _inclusions.AsEnumerable());  }
        }

        public async Task UpdateRules(string id, EventRuleDto rules)
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

            await Task.Run(() =>
            {
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
            });
        }

        public async Task<Tuple<bool, bool>> GetPrecondition(string id)
        {
            return await Task.Run(() =>
            {
                lock (_preconditions)
                {
                    return _preconditions[id];

                }
            });
        }

        public async Task<IEnumerable<String>> GetPreconditions()
        {
            return await Task.Run(() =>
            {
                lock (_preconditions)
                {
                    return _preconditions.Keys;
                }
            });
        }

        public async Task AddPrecondition(string key, Tuple<bool, bool> state)
        {
            await Task.Run(() =>
            {
                lock (_preconditions)
                {
                    _preconditions[key] = state;
                }
            });
        }

        public Task<EventStateDto> EventStateDto
        {
            get
            {
                return Task.Run(() =>
                {
                    lock (_preconditions)
                    {
                        return new EventStateDto
                        {
                            Executed = Executed,
                            Included = Included,
                            Pending = Pending,
                            Executable = _preconditions.Values.All(state => state.Item1 || state.Item2)
                        };
                    }
                });
            }
        }

        public async Task<IPEndPoint> GetEndPointFromId(string id)
        {
            return await Task.Run(() => EventEndPoints[id]);
        }

        public async Task<string> GetIdFromEndPoint(IPEndPoint endPoint)
        {
            return await Task.Run(() => EventIds[endPoint]);
        }

        public async Task RegisterIdWithEndPoint(string id, IPEndPoint endPoint)
        {
            await Task.Run(() =>
            {
                EventEndPoints.Add(id, endPoint);
                EventIds.Add(endPoint, id);
            });
        }

        public async Task<bool> KnowsId(string id)
        {
            return await Task.Run(() => EventEndPoints.ContainsKey(id));
        }

        public async Task RemoveIdAndEndPoint(string id)
        {
            await Task.Run(() =>
            {
                EventIds.Remove(EventEndPoints[id]);
                EventEndPoints.Remove(id);
            });
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
                    Conditions = Conditions.Result,
                    Exclusions = Exclusions.Result,
                    Responses = Responses.Result,
                    Inclusions = Inclusions.Result
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