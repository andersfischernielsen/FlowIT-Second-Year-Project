using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;

namespace Event.Models
{
    public class Logic : ILogic
    {
        #region Properties
        public Uri OwnUri
        {
            set { _inMemoryStorage.OwnUri = value; }
            get { return _inMemoryStorage.OwnUri; }
        }

        public string WorkflowId
        {
            set { _inMemoryStorage.WorkflowId = value; }
            get { return _inMemoryStorage.WorkflowId; }
        }

        public string EventId
        {
            set { _inMemoryStorage.EventId = value; }
            get { return _inMemoryStorage.EventId; }
        }

        public bool Executed
        {
            set { _inMemoryStorage.Executed = value; }
            get { return _inMemoryStorage.Executed; }
        }

        public bool Included
        {
            set { _inMemoryStorage.Included = value; }
            get { return _inMemoryStorage.Included; }
        }

        public bool Pending
        {
            set { _inMemoryStorage.Pending = value; }
            get { return _inMemoryStorage.Pending; }
        }

        public Task<HashSet<Uri>> Conditions
        {
            set { _inMemoryStorage.Conditions = value; }
            get { return _inMemoryStorage.Conditions; }
        }

        public Task<HashSet<Uri>> Responses
        {
            set { _inMemoryStorage.Responses = value; }
            get { return _inMemoryStorage.Responses; }
        }

        public Task<HashSet<Uri>> Exclusions
        {
            set { _inMemoryStorage.Exclusions = value; }
            get { return _inMemoryStorage.Exclusions; }
        }

        public Task<HashSet<Uri>> Inclusions
        {
            set { _inMemoryStorage.Inclusions = value; }
            get { return _inMemoryStorage.Inclusions; }
        }
        #endregion

        #region Init
        //Storage instance for getting and setting data.
        private readonly InMemoryStorage _inMemoryStorage;

        //Singleton instance.
        private static Logic _logic;

        public static Logic GetState()
        {
            return _logic ?? (_logic = new Logic());
        }

        private Logic()
        {
            _inMemoryStorage = new InMemoryStorage();
        }
        #endregion

        #region Rule Handling
        public async Task<bool> IsExecutable()
        {
            //If this event is excluded, return false.
            if (!Included)
            {
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

        public async Task UpdateRules(string id, EventRuleDto rules)
        {
            var endPoint = _inMemoryStorage.EventUris[id];
            if (endPoint == null)
            {
                throw new ArgumentException("Nonexistent id", id);
            }
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            UpdateRule(rules.Condition, endPoint, await Conditions);
            UpdateRule(rules.Exclusion, endPoint, await Exclusions);
            UpdateRule(rules.Inclusion, endPoint, await Inclusions);
            UpdateRule(rules.Response, endPoint, await Responses);
        }

        private static void UpdateRule(bool shouldAdd, Uri value, ISet<Uri> collection)
        {
            if (shouldAdd) collection.Add(value);
            else collection.Remove(value);
        }

        #endregion

        #region DTO Creation
        public async Task<IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>> GetNotifyDtos()
        {
            var result = new Dictionary<Uri, List<NotifyDto>>();

            foreach (var response in await Responses)
            {
                await AddNotifyDto(result, response, s => new PendingDto { Id = s });
            }

            foreach (var exclusion in await Exclusions)
            {
                await AddNotifyDto(result, exclusion, s => new ExcludeDto { Id = s });
            }

            foreach (var inclusion in await Inclusions)
            {
                await AddNotifyDto(result, inclusion, s => new IncludeDto { Id = s });
            }

            return (IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>) result;
        }

        private async Task AddNotifyDto<T>(IDictionary<Uri, List<NotifyDto>> dictionary, Uri uri, Func<string, T> creator)
            where T : NotifyDto
        {
            var dto = creator.Invoke(await _inMemoryStorage.GetIdFromUri(uri));

            if (dictionary.ContainsKey(uri)) { dictionary[uri].Add(dto); }
            else { dictionary.Add(uri, new List<NotifyDto> { dto }); }
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
                    Executable = await IsExecutable()
                });
            }
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
        #endregion

        #region URI Registering
        public Task RegisterIdWithUri(string id, Uri endPoint)
        {
            return Task.Run(() => _inMemoryStorage.RegisterIdWithUri(id, endPoint));
        }

        public Task<bool> KnowsId(string id)
        {
            return Task.Run(() => _inMemoryStorage.IdExists(id));
        }

        public Task RemoveIdAndUri(string id)
        {
            return Task.Run(() => _inMemoryStorage.RemoveIdAndUri(id));
        }
        #endregion
    }
}