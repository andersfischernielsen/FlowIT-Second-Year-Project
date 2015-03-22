using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
        private IEventStorage Storage { get; set; }
        public EventController()
        {
            Storage = InMemoryStorage.GetState();
        }

        
        private Uri GetUriOfEvent()
        {
            Uri result = null;
            if (HttpContext.Current != null)
            {
                result = HttpContext.Current.Request.UserHostAddress == "::1"
                    ? new Uri("http://localhost:13752")
                    : new Uri(String.Format("http://{0}:13752/", HttpContext.Current.Request.UserHostAddress));
            }
            return result;
        }

        #region State
        [Route("pending")]
        [HttpGet]
        public bool GetPending()
        {
            return Storage.Pending;
        }

        [Route("executed")]
        [HttpGet]
        public bool GetExecuted()
        {
            return Storage.Executed;
        }

        [Route("included")]
        [HttpGet]
        public bool GetIncluded()
        {
            return Storage.Included;
        }

        [Route("executable")]
        [HttpGet]
        public async Task<bool> GetExecutable()
        {
            return await ((InMemoryStorage)Storage).Executable();
        }
        #endregion

        #region EventEvent
        /// <summary>
        /// Get the entire state of the Event, including rules.
        /// </summary>
        /// <returns>A task resulting in a single EventDto which represents the Events current state.</returns>
        [Route("")]
        [HttpGet]
        public async Task<EventDto> GetEvent()
        {
            return await Storage.EventDto;
        }

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
            if (await Storage.KnowsId(id))
            {
                return BadRequest(string.Format("{0} already exists!", id));
            }

            await Storage.RegisterIdWithUri(id, GetUriOfEvent());

            await Storage.UpdateRules(id, ruleDto);

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
            if (!await Storage.KnowsId(id))
            {
                return BadRequest(string.Format("{0} does not exist!", id));
            }

            await Storage.UpdateRules(id, ruleDto);

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
            if (!await Storage.KnowsId(id))
            {
                return BadRequest(string.Format("{0} does not exist!", id));
            }

            await Storage.UpdateRules(id,
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
            await Storage.RemoveIdAndUri(id);

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
                Storage.Included = true;
            }
            if (exclude)
            {
                Storage.Included = false;
            }
            if (pending)
            {
                Storage.Pending = true;
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
            return await Storage.EventStateDto;
        }

        /// <summary>
        /// Executes this event. Only Clients should invoke this.
        /// </summary>
        /// <param name="execute">Whether to execute or not?</param>
        /// <returns>A Task resulting in an Http Result.</returns>
        [Route("{execute:bool}")]
        [HttpPut]
        public async Task<IHttpActionResult> Execute(bool execute)
        {
            if (execute)
            {
                if (!(await ((InMemoryStorage)Storage).Executable()))
                {
                    return BadRequest("Event is not currently executable.");
                }
                Storage.Executed = true;
                var notifyDtos = await Storage.GetNotifyDtos();
                Parallel.ForEach(notifyDtos, async pair =>
                {
                    await new EventCommunicator(pair.Key).SendNotify(pair.Value.ToArray());
                });
                return Ok(true);
            }
            // Todo: Is this what should happen when execute is false? Probably every condition should be notified?
            Storage.Executed = false;
            return Ok();
        }
        #endregion
    }
}