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
        public Uri OwnUri { get; set; }
        public string WorkflowId { get; set; }
        public string EventId { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }

        public async Task<bool> Executable()
        {
            //If this event is excluded, return false.
            if (!Included) {
                return false;
            }

            foreach (var condition in await Conditions)
            {
                IEventFromEvent eventCommunicator = new EventCommunicator(condition);
                var executed = await eventCommunicator.IsExecuted();
                var included = await eventCommunicator.IsIncluded();
                // If the condition-event is not executed and currently included.
                if (!executed || included)
                {
                    return false; //Don't check rest because this event is not executable.
                }
            }
            return true; // If all conditions are executed or excluded.
        }

        private Dictionary<string, Uri> EventUris { get; set; }
        private Dictionary<Uri, string> EventIds { get; set; }

        public static InMemoryStorage GetState()
        {
            return _inMemoryStorage ?? (_inMemoryStorage = new InMemoryStorage());
        }

        private InMemoryStorage()
        {
            EventUris = new Dictionary<string, Uri>();
            EventIds = new Dictionary<Uri, string>();
            Included = true;
        }

        public Task<HashSet<Uri>> Conditions { get; set; }
                    
        public Task<HashSet<Uri>> Responses { get; set; }
                    
        public Task<HashSet<Uri>> Exclusions { get; set; }
                    
        public Task<HashSet<Uri>> Inclusions { get; set; }

        public async Task<IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>> GetNotifyDtos()
        {

            var result = new Dictionary<Uri, List<NotifyDto>>();
            foreach (var response in await Responses)
            {
                await AddNotifyDto(result, response, s => new PendingDto {Id = s});
            }

            foreach (var exclusion in await Exclusions)
            {
                await AddNotifyDto(result, exclusion, s => new ExcludeDto {Id = s});
            }

            foreach (var inclusion in await Inclusions)
            {
                await AddNotifyDto(result, inclusion, s => new IncludeDto {Id = s});
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

            await UpdateRule(rules.Condition, endPoint, await Conditions);
            await UpdateRule(rules.Exclusion, endPoint, await Exclusions);
            await UpdateRule(rules.Inclusion, endPoint, await Inclusions);
            await UpdateRule(rules.Response, endPoint, await Responses);
        }

        private async static Task UpdateRule(bool shouldAdd, Uri value, ISet<Uri> collection)
        {
            if (shouldAdd) collection.Add(value);
            else collection.Remove(value);
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
    }
}