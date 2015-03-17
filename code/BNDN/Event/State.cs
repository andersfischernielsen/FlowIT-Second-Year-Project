using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;

namespace Event
{
    

    // Super hack! TODO: Database or equivalent.
    public class State
    {
        private static State _state;

        public static State GetState()
        {
            if (_state == null)
            {
                return _state = new State();
            }
            return _state;
        }

        public string Id { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }

        public HashSet<IPEndPoint> Conditions { get; set; }
        public HashSet<IPEndPoint> Responses { get; set; }
        public HashSet<IPEndPoint> Exclusions { get; set; }
        public HashSet<IPEndPoint> Inclusions { get; set; }

        private State()
        {
            Conditions = new HashSet<IPEndPoint>();
            Responses = new HashSet<IPEndPoint>();
            Exclusions = new HashSet<IPEndPoint>();
            Inclusions = new HashSet<IPEndPoint>();
            EventEndPoints = new Dictionary<string, IPEndPoint>();
            EventIds = new Dictionary<IPEndPoint, string>();
        }

        private readonly Dictionary<string, Tuple<bool, bool>> _preconditions = new Dictionary<string, Tuple<bool, bool>>();

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


        //TODO: IS THIS THE WAY TO GO?
        private Dictionary<string, IPEndPoint> EventEndPoints { get; set; }
        private Dictionary<IPEndPoint, string> EventIds { get; set; } 

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
                    Conditions = Conditions.ToList(),
                    Exclusions = Exclusions.ToList(),
                    Responses = Responses.ToList(),
                    Inclusions = Inclusions.ToList()
                });
            }
        }
    }
}