using System;
using System.Collections.Generic;
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
            set { Storage.OwnUri = value; }
            get { return Storage.OwnUri; }
        }

        public string WorkflowId
        {
            set { Storage.WorkflowId = value; }
            get { return Storage.WorkflowId; }
        }

        public string EventId
        {
            set { Storage.EventId = value; }
            get { return Storage.EventId; }
        }

        public string Name
        {
            set { Storage.Name = value; }
            get { return Storage.Name; }
        }

        public LockDto LockDto
        {
            set { Storage.LockDto = value; }
            get { return Storage.LockDto; }
        }

        public bool Executed
        {
            set { Storage.Executed = value; }
            get { return Storage.Executed; }
        }

        public bool Included
        {
            set { Storage.Included = value; }
            get { return Storage.Included; }
        }

        public bool Pending
        {
            set { Storage.Pending = value; }
            get { return Storage.Pending; }
        }

        public HashSet<Uri> Conditions
        {
            set { Storage.Conditions = value; }
            get { return Storage.Conditions; }
        }

        public HashSet<Uri> Responses
        {
            set { Storage.Responses = value; }
            get { return Storage.Responses; }
        }

        public HashSet<Uri> Exclusions
        {
            set { Storage.Exclusions = value; }
            get { return Storage.Exclusions; }
        }

        public HashSet<Uri> Inclusions
        {
            set { Storage.Inclusions = value; }
            get { return Storage.Inclusions; }
        }
        #endregion

        #region Init
        //Storage instance for getting and setting data.
        //TODO: This is a hack! The Storage shouldn't be accessible from outside of the logic.
        public readonly IEventStorage Storage;

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
            // TODO: Server address
            Storage = new InMemoryStorage();
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

            foreach (var condition in Conditions)
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
            await Task.Run(() =>
            {
                var uri = Storage.EventUris[id];
                if (uri == null)
                {
                    throw new ArgumentException("Nonexistent id", id);
                }
                if (rules == null)
                {
                    throw new ArgumentNullException("rules");
                }

                UpdateRule(rules.Condition, uri, Conditions);
                UpdateRule(rules.Exclusion, uri, Exclusions);
                UpdateRule(rules.Inclusion, uri, Inclusions);
                UpdateRule(rules.Response, uri, Responses);
            });
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

            foreach (var response in Responses)
            {
                await AddNotifyDto(result, response, s => new PendingDto { Id = s });
            }

            foreach (var exclusion in Exclusions)
            {
                await AddNotifyDto(result, exclusion, s => new ExcludeDto { Id = s });
            }

            foreach (var inclusion in Inclusions)
            {
                await AddNotifyDto(result, inclusion, s => new IncludeDto { Id = s });
            }

            return (IEnumerable<KeyValuePair<Uri, List<NotifyDto>>>)result;
        }

        public async Task AddNotifyDto<T>(IDictionary<Uri, List<NotifyDto>> dictionary, Uri uri, Func<string, T> creator)
            where T : NotifyDto
        {
            await Task.Run(() =>
            {
                var dto = creator.Invoke(Storage.GetIdFromUri(uri));

                if (dictionary.ContainsKey(uri))
                {
                    dictionary[uri].Add(dto);
                }
                else
                {
                    dictionary.Add(uri, new List<NotifyDto> {dto});
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
                    Executable = await IsExecutable()
                });
            }
        }

        public Task<EventDto> EventDto
        {
            get
            {
                return Task.Run(() => new EventDto
                {
                    EventId = EventId,
                    WorkflowId = WorkflowId,
                    Name = Name,
                    Pending = Pending,
                    Executed = Executed,
                    Included = Included,
                    Conditions = Conditions,
                    Exclusions = Exclusions,
                    Responses = Responses,
                    Inclusions = Inclusions
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
            return Task.Run(() => Storage.IdExists(id));
        }

        public Task RemoveIdAndUri(string id)
        {
            return Task.Run(() => Storage.RemoveIdAndUri(id));
        }
        #endregion


        public async Task ResetState()
        {
            await Task.Run(() =>
            {
                Storage.Name = null;
                Storage.EventId = null;
                Storage.WorkflowId = null;
                Storage.OwnUri = null;
            });
        }

        public bool CallerIsAllowedToOperate(EventAddressDto eventAddressDto)
        {
            if (LockDto == null) return true;
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
                throw new NullReferenceException("EventId was not null");
            }

            EventId = eventDto.EventId;
            WorkflowId = eventDto.WorkflowId;
            Name = eventDto.Name;
            Included = eventDto.Included;
            Pending = eventDto.Pending;
            Executed = eventDto.Executed;
            // TODO: Review if the awaiting done here is legal / intended or not, blame Morten!
            Inclusions = await Task.Run(() => new HashSet<Uri>(eventDto.Inclusions));
            Exclusions = await Task.Run(() => new HashSet<Uri>(eventDto.Exclusions));
            Conditions = await Task.Run(() => new HashSet<Uri>(eventDto.Conditions));
            Responses = await Task.Run(() => new HashSet<Uri>(eventDto.Responses));
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
            Inclusions = await Task.Run(() => new HashSet<Uri>(eventDto.Inclusions));
            Exclusions = await Task.Run(() => new HashSet<Uri>(eventDto.Exclusions));
            Conditions = await Task.Run(() => new HashSet<Uri>(eventDto.Conditions));
            Responses = await Task.Run(() => new HashSet<Uri>(eventDto.Responses));

           

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

        public bool IsLocked()
        {
            return LockDto != null;
        }
    }
}