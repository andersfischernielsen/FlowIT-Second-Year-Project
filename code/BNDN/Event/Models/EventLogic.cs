using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;

namespace Event.Models
{
    public class EventLogic : IEventLogic
    {
        #region Properties
        public Uri OwnUri
        {
            set { InMemoryStorage.OwnUri = value; }
            get { return InMemoryStorage.OwnUri; }
        }

        public string WorkflowId
        {
            set { InMemoryStorage.WorkflowId = value; }
            get { return InMemoryStorage.WorkflowId; }
        }

        public string EventId
        {
            set { InMemoryStorage.EventId = value; }
            get { return InMemoryStorage.EventId; }
        }

        public string Name
        {
            set { InMemoryStorage.Name = value; }
            get { return InMemoryStorage.Name; }
        }

        public LockDto LockDto
        {
            set { InMemoryStorage.LockDto = value; }
            get { return InMemoryStorage.LockDto; }
        }

        public bool Executed
        {
            set { InMemoryStorage.Executed = value; }
            get { return InMemoryStorage.Executed; }
        }

        public bool Included
        {
            set { InMemoryStorage.Included = value; }
            get { return InMemoryStorage.Included; }
        }

        public bool Pending
        {
            set { InMemoryStorage.Pending = value; }
            get { return InMemoryStorage.Pending; }
        }

        public Task<HashSet<Uri>> Conditions
        {
            set { InMemoryStorage.Conditions = value.Result; }
            get { return Task.Run(() => InMemoryStorage.Conditions); }
        }

        public Task<HashSet<Uri>> Responses
        {
            set { InMemoryStorage.Responses = value.Result; }
            get { return Task.Run(() => InMemoryStorage.Responses); }
        }

        public Task<HashSet<Uri>> Exclusions
        {
            set { InMemoryStorage.Exclusions = value.Result; }
            get { return Task.Run(() => InMemoryStorage.Exclusions); }
        }

        public Task<HashSet<Uri>> Inclusions
        {
            set { InMemoryStorage.Inclusions = value.Result; }
            get { return Task.Run(() => InMemoryStorage.Inclusions); }
        }
        #endregion

        #region Init
        //Storage instance for getting and setting data.
        //TODO: This is a hack! The Storage shouldn't be accessible from outside of the logic.
        public readonly InMemoryStorage InMemoryStorage;

        //Singleton instance.
        private static EventLogic _eventLogic;

        // ServerCommunicator
        private IServerFromEvent ServerCommunicator { get; set; }

        public static EventLogic GetState()
        {
            return _eventLogic ?? (_eventLogic = new EventLogic());
        }

        private EventLogic()
        {
            InMemoryStorage = new InMemoryStorage();
            // TODO: Server address
            ServerCommunicator = new ServerCommunicator("http://localhost:13768/", EventId, WorkflowId);
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
            var endPoint = InMemoryStorage.EventUris[id];
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

            return (IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>)result;
        }

        public async Task AddNotifyDto<T>(IDictionary<Uri, List<NotifyDto>> dictionary, Uri uri, Func<string, T> creator)
            where T : NotifyDto
        {
            var dto = creator.Invoke(InMemoryStorage.GetIdFromUri(uri));

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
                    EventId = EventId,
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

        #region URI Registration
        public Task RegisterIdWithUri(string id, Uri endPoint)
        {
            return Task.Run(() => RegisterIdWithUri(id, endPoint));
        }

        public Task<bool> KnowsId(string id)
        {
            return Task.Run(() => InMemoryStorage.IdExists(id));
        }

        public Task RemoveIdAndUri(string id)
        {
            return Task.Run(() => InMemoryStorage.RemoveIdAndUri(id));
        }
        #endregion


        public Task ResetState()
        {
            return Task.Run(() =>
            {
                InMemoryStorage.Name = null;
                InMemoryStorage.EventId = null;
                InMemoryStorage.WorkflowId = null;
                InMemoryStorage.OwnUri = null;
            });
        }

        public bool IsAllowedToOperate(EventAddressDto eventAddressDto)
        {
            // TODO: Consider implementing an Equals() method on EventAddressDto
            return LockDto.LockOwner.Equals(eventAddressDto.Id);
        }

        // TODO: InitializeEvent and UpdateEvent has a lot of duplicated code, will look into this later
        public async Task InitializeEvent(EventDto eventDto, Uri ownUri)
        {
            if (eventDto == null)
            {
                throw new NullReferenceException("Provided EventDto was null");
            }
            if (EventId != null)
            {
                throw new NullReferenceException("EventId was null");
            }

            EventId = eventDto.EventId;
            WorkflowId = eventDto.WorkflowId;
            Name = eventDto.Name;
            Included = eventDto.Included;
            Pending = eventDto.Pending;
            Executed = eventDto.Executed;
            Inclusions = Task.Run(() => new HashSet<Uri>(eventDto.Inclusions));
            Exclusions = Task.Run(() => new HashSet<Uri>(eventDto.Exclusions));
            Conditions = Task.Run(() => new HashSet<Uri>(eventDto.Conditions));
            Responses = Task.Run(() => new HashSet<Uri>(eventDto.Responses));
            OwnUri = ownUri;

            var dto = new EventAddressDto
            {
                Id = EventId,
                Uri = OwnUri
            };

           var otherEvents = await ServerCommunicator.PostEventToServer(dto);

            /*
            foreach (var otherEvent in otherEvents)
            {
                // Todo register self with other Events.
                await Logic.RegisterIdWithUri(otherEvent.Id, otherEvent.Uri);
            }*/
        }

        public async Task UpdateEvent(EventDto eventDto, Uri ownUri)
        {

            if (eventDto == null)
            {
                throw new NullReferenceException("Provided EventDto was null");
            }
            if (EventId != null)
            {
                throw new NullReferenceException("EventId was null");
            }

            if (EventId != eventDto.EventId || WorkflowId != eventDto.WorkflowId)
            {
                //Todo remove from server and add again.
            }

            EventId = eventDto.EventId;
            WorkflowId = eventDto.WorkflowId;
            Name = eventDto.Name;
            Included = eventDto.Included;
            Pending = eventDto.Pending;
            Executed = eventDto.Executed;
            Inclusions = Task.Run(() => new HashSet<Uri>(eventDto.Inclusions));
            Exclusions = Task.Run(() => new HashSet<Uri>(eventDto.Exclusions));
            Conditions = Task.Run(() => new HashSet<Uri>(eventDto.Conditions));
            Responses = Task.Run(() => new HashSet<Uri>(eventDto.Responses));

           

            // Todo: This should not be necessary..
            OwnUri = ownUri;

            var dto = new EventAddressDto
            {
                Id = EventId,
                Uri = OwnUri
            };

            var otherEvents = await ServerCommunicator.PostEventToServer(dto);


            // Todo clear old registered events!
            foreach (var otherEvent in otherEvents)
            {
                await RegisterIdWithUri(otherEvent.Id, otherEvent.Uri);
            }
        }

        public async Task DeleteEvent()
        {
            if (EventId == null)
            {
                throw new NullReferenceException();
            }

            await ServerCommunicator.DeleteEventFromServer();
            await ResetState();
        }
    }
}