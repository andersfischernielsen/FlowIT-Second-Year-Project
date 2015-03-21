using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;

namespace Event.Models
{
    public class InMemoryStorage : IEventStorage
    {
        private static InMemoryStorage _inMemoryStorage;

        public string WorkflowId { get; set; }
        public string EventId { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }

        public async Task<bool> Executable()
        {
            IEventFromEvent eventCommunicator = new EventCommunicator();
            foreach (var condition in await Conditions)
            {
                var e = await eventCommunicator.GetEvent(condition);
                // If the condition-event is not executed and currently included
                if (!e.Executed || e.Included)
                {
                    return false; //Don't check rest because this event is not executable.
                }
            }
            return true; // If all conditions are executed or excluded.
        }

        private readonly HashSet<Uri> _conditions;
        private readonly HashSet<Uri> _responses;
        private readonly HashSet<Uri> _exclusions;
        private readonly HashSet<Uri> _inclusions;

        private Dictionary<string, Uri> EventUris { get; set; }
        private Dictionary<Uri, string> EventIds { get; set; }

        private InMemoryStorage()
        {
            _conditions = new HashSet<Uri>();
            _responses = new HashSet<Uri>();
            _exclusions = new HashSet<Uri>();
            _inclusions = new HashSet<Uri>();
            EventUris = new Dictionary<string, Uri>();
            EventIds = new Dictionary<Uri, string>();
        }

        public Task<IEnumerable<Uri>> Conditions
        {
            get { return Task.Run(() => _conditions.AsEnumerable()); }
        }

        public Task<IEnumerable<Uri>> Responses
        {
            get { return Task.Run(() => _responses.AsEnumerable()); }
        }

        public Task<IEnumerable<Uri>> Exclusions
        {
            get { return Task.Run(() => _exclusions.AsEnumerable()); }
        }

        public Task<IEnumerable<Uri>> Inclusions
        {
            get { return Task.Run(() => _inclusions.AsEnumerable()); }
        }

        public async Task<IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>> GetNotifyDtos()
        {

            var result = new Dictionary<Uri, List<NotifyDto>>();
            foreach (var response in _responses)
            {
                await AddNotifyDto(result, response, s => new PendingDto(s));
            }

            foreach (var exclusion in _exclusions)
            {
                await AddNotifyDto(result, exclusion, s => new ExcludeDto(s));
            }

            foreach (var inclusion in _inclusions)
            {
                await AddNotifyDto(result, inclusion, s => new IncludeDto(s));
            }

            return (IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>)result;
        }

        private async Task AddNotifyDto<T>(IDictionary<Uri, List<NotifyDto>> dictionary, Uri uri, Func<string, T> creator)
            where T : NotifyDto
        {
            T dto = creator.Invoke(await GetIdFromUri(uri));
            if (dictionary.ContainsKey(uri))
            {
                dictionary[uri].Add(dto);
            }
            else
            {
                dictionary.Add(uri, new List<NotifyDto> { dto });
            }
        }

        public async Task UpdateRules(string id, EventRuleDto rules)
        {
            var endPoint = EventUris[id];
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

        public Task<EventStateDto> EventStateDto
        {
            get
            {
                return Task.Run(async () => new EventStateDto
                {
                    Executed = Executed,
                    Included = Included,
                    Pending = Pending,
                    Executable = await Executable()
                });
            }
        }

        public async Task<Uri> GetUriFromId(string id)
        {
            return await Task.Run(() => EventUris[id]);
        }

        public async Task<string> GetIdFromUri(Uri endPoint)
        {
            return await Task.Run(() => EventIds[endPoint]);
        }

        public async Task RegisterIdWithUri(string id, Uri endPoint)
        {
            await Task.Run(() =>
            {
                EventUris.Add(id, endPoint);
                EventIds.Add(endPoint, id);
            });
        }

        public async Task<bool> KnowsId(string id)
        {
            return await Task.Run(() => EventUris.ContainsKey(id));
        }

        public async Task RemoveIdAndUri(string id)
        {
            await Task.Run(() =>
            {
                EventIds.Remove(EventUris[id]);
                EventUris.Remove(id);
            });
        }

        public Task<EventDto> EventDto
        {
            get
            {
                return Task.Run(async () => new EventDto
                {
                    Id = EventId,
                    Pending = Pending,
                    Executed = Executed,
                    Included = Included,
                    Conditions = await Conditions,
                    Exclusions = await Exclusions,
                    Responses = await Responses,
                    Inclusions = await Inclusions
                });
            }
        }

        public static InMemoryStorage GetState()
        {
            return _inMemoryStorage ?? (_inMemoryStorage = new InMemoryStorage());
        }
    }
}