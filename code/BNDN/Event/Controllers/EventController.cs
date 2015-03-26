using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Interfaces;
using Event.Models;

namespace Event.Controllers
{
    /// <summary>
    /// The EventController is responsible for handling the Api requests on the {host}/event/* service.
    /// It must both handle incoming requests from other Events and incoming requests from Clients.
    /// </summary>
    [RoutePrefix("event")]
    public class EventController : ApiController
    {
        private IEventLogic Logic { get; set; }
        public EventController()
        {
            Logic = EventLogic.GetState();
        }

        #region State
        [Route("pending")]
        [HttpGet]
        public bool GetPending()
        {
            return Logic.Pending;
        }

        [Route("executed")]
        [HttpGet]
        public bool GetExecuted()
        {
            return Logic.Executed;
        }

        [Route("included")]
        [HttpGet]
        public bool GetIncluded()
        {
            return Logic.Included;
        }

        [Route("executable")]
        [HttpGet]
        public async Task<bool> GetExecutable()
        {
            return await ((EventLogic)Logic).IsExecutable();
        }
        #endregion

        #region EventDto
        /// <summary>
        /// Get the entire state of the Event, including rules.
        /// </summary>
        /// <returns>A task resulting in a single EventDto which represents the Events current state.</returns>
        [Route("")]
        [HttpGet]
        public async Task<EventDto> GetEvent()
        {
            return await Logic.EventDto;
        }

        [Route("")]
        [HttpPost]
        public async Task PostEvent([FromBody] EventDto eventDto)
        {
            // Todo: Fix
            var logic = (EventLogic)Logic;
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            if (eventDto == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data was null"));
            }
            if (logic.EventId != null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Event is already running!"));
            }

            logic.EventId = eventDto.EventId;
            logic.WorkflowId = eventDto.WorkflowId;
            logic.Name = eventDto.Name;
            logic.Included = eventDto.Included;
            logic.Pending = eventDto.Pending;
            logic.Executed = eventDto.Executed;
            logic.Inclusions = Task.Run(() => new HashSet<Uri>(eventDto.Inclusions));
            logic.Exclusions = Task.Run(() => new HashSet<Uri>(eventDto.Exclusions));
            logic.Conditions = Task.Run(() => new HashSet<Uri>(eventDto.Conditions));
            logic.Responses = Task.Run(() => new HashSet<Uri>(eventDto.Responses)); 
            logic.OwnUri = new Uri(Request.RequestUri.Authority);

            var dto = new EventAddressDto
            {
                Id = logic.EventId,
                Uri = logic.OwnUri
            };

            // Todo: Server address.
            IServerFromEvent commuicator = new ServerCommunicator("http://serveraddress.azurewebsites.net", logic.EventId, logic.WorkflowId);

            var otherEvents = await commuicator.PostEventToServer(dto);

            foreach (var otherEvent in otherEvents)
            {
                await Logic.RegisterIdWithUri(otherEvent.Id, otherEvent.Uri);
            }
        }

        [Route("")]
        [HttpPut]
        public async Task PutEvent([FromBody] EventDto eventDto)
        {
            var logic = (EventLogic)Logic;
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            if (eventDto == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data was null"));
            }
            if (logic.EventId == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Event is not initialized!"));
            }
            if (logic.EventId != eventDto.EventId || logic.WorkflowId != eventDto.WorkflowId)
            {
                //Todo remove from server and add again.
            }

            logic.EventId = eventDto.EventId;
            logic.WorkflowId = eventDto.WorkflowId;
            logic.Name = eventDto.Name;
            logic.Included = eventDto.Included;
            logic.Pending = eventDto.Pending;
            logic.Executed = eventDto.Executed;
            logic.Inclusions = Task.Run(() => new HashSet<Uri>(eventDto.Inclusions));
            logic.Exclusions = Task.Run(() => new HashSet<Uri>(eventDto.Exclusions));
            logic.Conditions = Task.Run(() => new HashSet<Uri>(eventDto.Conditions));
            logic.Responses = Task.Run(() => new HashSet<Uri>(eventDto.Responses));

            // Todo: This should not be necessary..
            logic.OwnUri = new Uri(Request.RequestUri.Authority);

            var dto = new EventAddressDto
            {
                Id = logic.EventId,
                Uri = logic.OwnUri
            };

            // Todo: Server address.
            IServerFromEvent commuicator = new ServerCommunicator("http://serveraddress.azurewebsites.net", logic.EventId, logic.WorkflowId);

            var otherEvents = await commuicator.PostEventToServer(dto);


            // Todo clear old registered events!
            foreach (var otherEvent in otherEvents)
            {
                await Logic.RegisterIdWithUri(otherEvent.Id, otherEvent.Uri);
            }
        }

        [Route("")]
        [HttpDelete]
        public async Task DeleteEvent()
        {
            var logic = (EventLogic) Logic;
            if (logic.EventId == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Event is not initialized!"));
            }

            // Todo: Server address.
            IServerFromEvent commuicator = new ServerCommunicator("http://serveraddress.azurewebsites.net", logic.EventId, logic.WorkflowId);

            await commuicator.DeleteEventFromServer();

            await Logic.ResetState();
        }
        #endregion

        #region EventEvent
        #region Rules
        /// <summary>
        /// Add a rule to this Event.
        /// </summary>
        /// <param name="id">The id of the calling Event.</param>
        /// <param name="ruleDto">A dto representing the rules which should be between this event and the calling event.</param>
        /// <returns>A task resulting in a Http Result.</returns>
        [Route("rules/{id}")]
        [HttpPost]
        public async Task<IHttpActionResult> PostRules(string id, [FromBody] EventRuleDto ruleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (ruleDto == null)
            {
                return BadRequest("Request requires data");
            }

            // if new entry, add Event to the endPoints-table.
            if (await Logic.KnowsId(id))
            {
                return BadRequest(string.Format("{0} already exists!", id));
            }

            await Logic.UpdateRules(id, ruleDto);

            return Ok();
        }

        /// <summary>
        /// Updates the rules between this Event and the caller.
        /// </summary>
        /// <param name="id">The id of the calling Event.</param>
        /// <param name="ruleDto">The complete set of updated rules. 
        /// If existing rules are not in the dto they will be removed.</param>
        /// <returns>A task resulting in a Http Result.</returns>
        [Route("rules/{id}")]
        [HttpPut]
        public async Task<IHttpActionResult> PutRules(string id, [FromBody] EventRuleDto ruleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (ruleDto == null)
            {
                return BadRequest("Request requires data");
            }

            // If the id is not known to this event, the PUT-call shall fail!
            if (! await Logic.KnowsId(id))
            {
                return BadRequest(string.Format("{0} does not exist!", id));
            }

            await Logic.UpdateRules(id, ruleDto);

            return Ok();
        }

        /// <summary>
        /// Deletes all rules associated with the Event with the given id.
        /// </summary>
        /// <param name="id">The id of the calling Event.</param>
        /// <returns>A task resulting in an Http Result.</returns>
        [Route("rules/{id}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteRules(string id)
        {
            if (!await Logic.KnowsId(id))
            {
                return BadRequest(string.Format("{0} does not exist!", id));
            }

            await Logic.UpdateRules(id,
                // Set all states to false to remove them from storage.
                // This effectively removes all rules associated with the given id.
                // Possibly - to save memory - it could be null instead.
                // Todo: Read above
                new EventRuleDto
                {
                    Condition = false,
                    Exclusion = false,
                    Inclusion = false,
                    Response = false
                });
            // Remove the id because it is no longer associated with any rules.
            await Logic.RemoveIdAndUri(id);

            return Ok();
        }
        #endregion

        [Route("notify")]
        [HttpPut]
        public async Task<IHttpActionResult> PutNotify([FromBody] IEnumerable<NotifyDto> dtos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var notifyDtos = dtos as IList<NotifyDto> ?? dtos.ToList();
            var include = notifyDtos.OfType<IncludeDto>().Any();
            var exclude = notifyDtos.OfType<ExcludeDto>().Any();
            var pending = notifyDtos.OfType<PendingDto>().Any();

            if (include && exclude)
            {
                return BadRequest("Notification must not contain both include and exclude!");
            }
            if (include)
            {
                Logic.Included = true;
            }
            if (exclude)
            {
                Logic.Included = false;
            }
            if (pending)
            {
                Logic.Pending = true;
            }
            // Todo: Await something sensible when connected to database.
            return await Task.Run(() => Ok());
        }
        #endregion

        #region ClientEvent
        /// <summary>
        /// Returns the current state of the events.
        /// </summary>
        /// <returns>A Task resulting in an EventStateDto which contains 3 
        /// booleans with the current state of the Event, plus a 4th boolean 
        /// which states whether the Event is currently executable</returns>
        [Route("state")]
        [HttpGet]
        public async Task<EventStateDto> GetState()
        {
            return await Logic.EventStateDto;
        }

        /// <summary>
        /// Executes this event. Only Clients should invoke this.
        /// todo: Should be able to return something to the caller.
        /// </summary>
        /// <returns>A Task resulting in an Http Result.</returns>
        [Route("executed")]
        [HttpPut]
        public async Task<IHttpActionResult> Execute()
        {
            if (!(await ((EventLogic)Logic).IsExecutable()))
            {
                return BadRequest("Event is not currently executable.");
            }
            Logic.Executed = true;
            var notifyDtos = await Logic.GetNotifyDtos();
            Parallel.ForEach(notifyDtos, async pair =>
            {
                await new EventCommunicator(pair.Key).SendNotify(pair.Value.ToArray());
            });
            return Ok(true);
        }
        #endregion
    }
}