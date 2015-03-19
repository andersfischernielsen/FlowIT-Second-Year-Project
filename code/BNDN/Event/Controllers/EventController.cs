using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
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
        private IEventCommunicator Communicator { get; set; }
        public EventController()
        {
            Storage = InMemoryStorage.GetState();
            // Todo: Use an actual implementation of IEventCommunicator.
            Communicator = null;
        }

        #region EventEvent
        /// <summary>
        /// Get the entire state of the Event, including rules.
        /// </summary>
        /// <returns>A task resulting in a single EventDto which represents the Events current state.</returns>
        [HttpGet]
        public async Task<EventDto> Get()
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

            // The following lines is pretty much a hack to get the IPv4-address of the caller.
            var addresses = (await Dns.GetHostAddressesAsync(Request.RequestUri.Host))
                .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                .ToArray();
            if (!addresses.Any() || addresses.Length > 1)
            {
                throw new Exception("Bad address!" + addresses.Length);
            }

            var endPoint = new IPEndPoint(addresses[0], Request.RequestUri.Port);
            await Storage.RegisterIdWithEndPoint(id, endPoint);

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
                new EventRuleDto
                {
                    Condition = false,
                    Exclusion = false,
                    Inclusion = false,
                    Response = false
                });
            // Remove the id because it is no longer associated with any rules.
            await Storage.RemoveIdAndEndPoint(id);

            return Ok();
        }
        #endregion
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
        [HttpPut]
        public async Task<IHttpActionResult> Execute(bool execute)
        {
            if (execute)
            {
                if (!(await (Storage.EventStateDto)).Executable)
                {
                    return BadRequest("Event is not currently executable.");
                }
                Storage.Executed = true;
                var notifyDtos = await Storage.GetNotifyDtos();
                foreach (var pair in notifyDtos)
                {
                    Communicator.SendNotify(pair.Key, pair.Value.ToArray());
                }
                return Ok(true);
            }
            // Todo: Is this what should happen when execute is false? Probably every condition should be notified?
            Storage.Executed = false;
            return Ok();
        }
        #endregion
    }
}