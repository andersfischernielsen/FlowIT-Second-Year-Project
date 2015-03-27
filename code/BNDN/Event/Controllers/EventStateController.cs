using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using System.Net;
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
        public bool GetPending([FromBody] EventAddressDto eventAddressDto)
        {
            if (!Logic.IsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot access this property. The event is locked."));
            }
            return Logic.Pending;

        }



        [Route("event/executed")]
        [HttpGet]
        public bool GetExecuted([FromBody] EventAddressDto eventAddressDto)
        {
            if (!Logic.IsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot access this property. The event is locked."));
            }
            return Logic.Executed;
        }


        [Route("event/included")]
        [HttpGet]
        public bool GetIncluded([FromBody] EventAddressDto eventAddressDto)
        {
            if (!Logic.IsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot access this property. The event is locked."));
            }
            return Logic.Included;
        }

        [Route("event/executable")]
        [HttpGet]
        public async Task<bool> GetExecutable([FromBody] EventAddressDto eventAddressDto)
        {
            if (!Logic.IsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot access this property. The event is locked."));
            }
            return await Logic.IsExecutable();
        }


        /// <summary>
        /// Returns the current state of the events.
        /// </summary>
        /// <returns>A Task resulting in an EventStateDto which contains 3 
        /// booleans with the current state of the Event, plus a 4th boolean 
        /// which states whether the Event is currently executable</returns>
        [Route("event/state")]
        [HttpGet]
        public async Task<EventStateDto> GetState([FromBody] EventAddressDto eventAddressDto)
        {
            if (!Logic.IsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot access this property. The event is locked."));
            }
            return await Logic.EventStateDto;
        }
        #endregion

        // TODO: We need to decide on a consistent way of updating these values; a) the (bool) value in body or b) the bool value in the url...
        #region PUT-requests
        /// <summary>
        /// Executes this event. Only Clients should invoke this.
        /// todo: Should be able to return something to the caller.
        /// </summary>
        [Route("event/executed")]
        [HttpPut]
        public async Task<IHttpActionResult> Execute([FromBody] bool execute)
        {
            if (!(await Logic.IsExecutable()))
            {
                return BadRequest("Event is not currently executable.");
            }

            LockLogic ll = new LockLogic();
            if (await ll.LockAll())
            {
                var notifyDtos = await Logic.GetNotifyDtos();

                //TODO: Det her er dårlig stil åbenbart: http://stackoverflow.com/questions/23137393/parallel-foreach-and-async-await-issue. Ved ikke hvad løsningen er dog
                Parallel.ForEach(notifyDtos, async pair =>
                {
                    await new EventCommunicator(pair.Key).SendNotify(pair.Value.ToArray());
                });

                Logic.Executed = true;

                if (!await ll.UnlockAll())
                {
                    //Kunne ikke unlocke alt, hvad skal der ske?
                }
                return Ok(true);
            }
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Another transaction is going on, try again later"));
        }


        [Route("event/included/{boolValueForIncluded}")]
        [HttpPut]
        public void UpdateIncluded([FromBody] EventAddressDto eventAddressDto, bool boolValueForIncluded)
        {
            if (!Logic.IsAllowedToOperate(eventAddressDto))
            {
               throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This event is already locked by someone else."));
            }
            Logic.Included = boolValueForIncluded;
        }

        [Route("event/pending/{boolValueForPending}")]
        [HttpPut]
        public void UpdatePending([FromBody] EventAddressDto eventAddressDto, bool boolValueForPending)
        {
            if (!Logic.IsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "This event is already locked by someone else."));
            }
            Logic.Pending = boolValueForPending;
        }

        #endregion

        #region Lock-related
        [Route("Event/lock")]
        [HttpPost]
        public void Lock([FromBody] LockDto lockDto)
        {
            if (Logic.LockDto != null)
            {
                // There cannot be set a new lock, since a lock is already set.
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Lock could not be acquired. Event is already locked."));
            }
            else if (lockDto == null)
            {
                // Caller provided a null LockDto
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Lock could not be set. An empty lock was provided."));
            }
            else
            {
                Logic.LockDto = lockDto;
            }
        }

        [Route("Event/lock")]
        [HttpDelete]
        public void Unlock([FromBody] EventAddressDto eventAddressDto)
        {
            if(!Logic.IsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Lock could be unlocked. Event was locked by someone else."));
            }
            else
            {
                Logic.LockDto = null;
            }
        }
        #endregion
    }
}
