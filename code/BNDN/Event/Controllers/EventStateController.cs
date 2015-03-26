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
    /// EventStateController handles GET- and PUT-requests on Event's state
    /// </summary>
    public class EventStateController : ApiController
    {

        private IEventStorage Storage { get; set; }

        public EventStateController()
        {
            // Fetches Singleton-storage
            Storage = InMemoryStorage.GetState();
        }

        #region GET-requests
        [Route("event/pending")]
        [HttpGet]
        public bool GetPending()
        {
            return Storage.Pending;
        }



        [Route("event/executed")]
        [HttpGet]
        public bool GetExecuted()
        {
            return Storage.Executed;
        }


        [Route("event/included")]
        [HttpGet]
        public bool GetIncluded()
        {
            return Storage.Included;
        }

        [Route("event/executable")]
        [HttpGet]
        public async Task<bool> GetExecutable()
        {
            return await ((InMemoryStorage)Storage).Executable();
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
            return await Storage.EventStateDto;
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


        [Route("event/included/{boolValueForIncluded}")]
        [HttpPut]
        public void UpdateIncluded(bool boolValueForIncluded)
        {
            Storage.Included = boolValueForIncluded;
        }

        [Route("event/pending/{boolValueForPending}")]
        [HttpPut]
        public void UpdatePending(bool boolValueForPending)
        {
            Storage.Pending = boolValueForPending;
        }


        [Route("event/executed/{boolValueForExecuted}")]
        [HttpPut]
        public void UpdateExecuted(bool boolValueForExecuted)
        {
            Storage.Executed = boolValueForExecuted;
        }

        #endregion

    }
}
