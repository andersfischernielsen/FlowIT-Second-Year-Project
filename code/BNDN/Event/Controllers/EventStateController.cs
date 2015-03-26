using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Interfaces;
using Event.Models;

namespace Event.Controllers
{
    /// <summary>
    /// EventStateController handles GET- and PUT-requests on Event's state
    /// </summary>
    public class EventStateController : ApiController
    {

        private IEventLogic Logic { get; set; }

        public EventStateController()
        {
            // Fetches Singleton-storage
            Logic = EventLogic.GetState();
        }

        #region GET-requests
        [Route("event/pending")]
        [HttpGet]
        public bool GetPending()
        {
            return Logic.Pending;
        }



        [Route("event/executed")]
        [HttpGet]
        public bool GetExecuted()
        {
            return Logic.Executed;
        }


        [Route("event/included")]
        [HttpGet]
        public bool GetIncluded()
        {
            return Logic.Included;
        }

        [Route("event/executable")]
        [HttpGet]
        public async Task<bool> GetExecutable()
        {
            return await ((EventLogic)Logic).IsExecutable();
        }


        /// <summary>
        /// Returns the current state of the events.
        /// </summary>
        /// <returns>A Task resulting in an EventStateDto which contains 3 
        /// booleans with the current state of the Event, plus a 4th boolean 
        /// which states whether the Event is currently executable</returns>
        [Route("event/state")]
        [HttpGet]
        public async Task<EventStateDto> GetState()
        {
            return await Logic.EventStateDto;
        }
        #endregion


        #region PUT-requests
        /// <summary>
        /// Executes this event. Only Clients should invoke this.
        /// todo: Should be able to return something to the caller.
        /// </summary>
        [Route("event/executed")]
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


        [Route("event/included/{boolValueForIncluded}")]
        [HttpPut]
        public void UpdateIncluded(bool boolValueForIncluded)
        {
            Logic.Included = boolValueForIncluded;
        }

        [Route("event/pending/{boolValueForPending}")]
        [HttpPut]
        public void UpdatePending(bool boolValueForPending)
        {
            Logic.Pending = boolValueForPending;
        }


        [Route("event/executed/{boolValueForExecuted}")]
        [HttpPut]
        public void UpdateExecuted(bool boolValueForExecuted)
        {
            Logic.Executed = boolValueForExecuted;
        }

        #endregion

    }
}
