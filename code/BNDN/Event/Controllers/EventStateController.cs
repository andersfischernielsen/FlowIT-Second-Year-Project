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
    /// EventStateController handles manipulations on Event't State, including locks on Event
    /// </summary>
    public class EventStateController : ApiController
    {

        private IEventLogic Logic { get; set; }

        public EventStateController()
        {
            // Fetches Singleton Logic-layer
            Logic = EventLogic.GetState();
        }

        #region GET-requests
        /// <summary>
        /// GetPending returns the (bool) value of Event's pending state. Callers of this method must identify themselves
        /// through an EventAddressDto
        /// </summary>
        /// <param name="eventAddressDto">Used as a representation for caller of this method</param>
        /// <returns>Event's pending (bool) value</returns>
        [Route("event/pending")]
        [HttpGet]
        public bool GetPending([FromBody] EventAddressDto eventAddressDto)
        {
            // Check is made to see if the caller is allowed to execute this method at the moment
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot access this property. The event is locked."));
            }
            return Logic.Pending;
        }

        /// <summary>
        /// GetExecuted returns the Event's current (bool) Executed value. 
        /// </summary>
        /// <param name="eventAddressDto">Content should represent the caller of this method</param>
        /// <returns>Event's current Executed value</returns>
        [Route("event/executed")]
        [HttpGet]
        public bool GetExecuted([FromBody] EventAddressDto eventAddressDto)
        {
            // Check is made to see if caller is allowed to execute this method at the moment. 
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "Cannot access this property. The event is locked."));
            }
            return Logic.Executed;
        }

        /// <summary>
        /// GetIncluded returns Event's current value for Included (bool). 
        /// </summary>
        /// <param name="eventAddressDto">Content should represent caller of the method.</param>
        /// <returns>Current value of Event's (bool) Included value</returns>
        [Route("event/included")]
        [HttpGet]
        public bool GetIncluded([FromBody] EventAddressDto eventAddressDto)
        {
            // Check is made to see if caller is allowed to execute this method
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "Cannot access this property. The event is locked."));
            }
            return Logic.Included;
        }

        /// <summary>
        /// Returns Event's current value of Executable.
        /// </summary>
        /// <param name="eventAddressDto">Content should represent caller</param>
        /// <returns>Current value of Event's Executable</returns>
        [Route("event/executable")]
        [HttpGet]
        public async Task<bool> GetExecutable([FromBody] EventAddressDto eventAddressDto)
        {
            // Check is made to see if caller is allowed to execute this method (at the moment)
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "Cannot access this property. The event is locked."));
            }
            return await Logic.IsExecutable();
        }

        /// <summary>
        /// Returns the current state of the events.
        /// </summary>
        /// <param name="eventAddressDto">Content of this should represent caller</param>
        /// <returns>A Task resulting in an EventStateDto which contains 3 
        /// booleans with the current state of the Event, plus a 4th boolean 
        /// which states whether the Event is currently executable</returns>
        [Route("event/state")]
        [HttpGet]
        public async Task<EventStateDto> GetState([FromBody] EventAddressDto eventAddressDto)
        {
            // Check is made to see whether caller is allowed to execute this method at the moment
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "Cannot access this property. The event is locked."));
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
        /// <param name="execute">Must be set to true; will result in BadRequest otherwise.</param>
        /// <returns></returns>
        [Route("event/executed")]
        [HttpPut]
        public async Task Execute([FromBody] bool execute)
        {
            // Check that caller knows what it is doing
            if (!execute)
            {
                BadRequest("Execute cannot be undone by supplying (bool) execute with a value of false");
            }

            // Check if Event is currently locked
            if (Logic.IsLocked())
            {
                BadRequest("Event is currently locked");
            }
            // Check whether Event can be executed at the moment
            if (!(await Logic.IsExecutable()))
            {
                BadRequest("Event is not currently executable.");
            }
            
            // TODO: Is this too early to set it to true; have we (at this point in this mthod) actually executed...? 
            Logic.Executed = true;

            // Retrieve info about the ones we need to notify about this Event having executed
            // TODO: Discuss: Wouldn't it make more sense having the following lines refactored into the logic - layer
            var notifyDtos = await Logic.GetNotifyDtos();
            Parallel.ForEach(notifyDtos, async pair =>
            {
                await new EventCommunicator(pair.Key).SendNotify(pair.Value.ToArray());
            });

            Ok(true);
        }


        /// <summary>
        /// Updates Event's current (bool) value for Included
        /// </summary>
        /// <param name="eventAddressDto">Content should represent caller. Used to identify caller.</param>
        /// <param name="boolValueForIncluded">The value that Included should be set to</param>
        [Route("event/included/{boolValueForIncluded}")]
        [HttpPut]
        public void UpdateIncluded([FromBody] EventAddressDto eventAddressDto, bool boolValueForIncluded)
        {
            // Check to see if caller is currently allowed to execute this method
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
               throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                   "This event is already locked by someone else."));
            }
            Logic.Included = boolValueForIncluded;
        }

        /// <summary>
        /// Updates Event's current (bool) value for Pending
        /// </summary>
        /// <param name="eventAddressDto">Content should represent caller.</param>
        /// <param name="boolValueForPending">The value Pending should be set to</param>
        [Route("event/pending/{boolValueForPending}")]
        [HttpPut]
        public void UpdatePending([FromBody] EventAddressDto eventAddressDto, bool boolValueForPending)
        {
            // Check if caller is allowed to execute this method at the moment
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "This event is already locked by someone else."));
            }
            Logic.Pending = boolValueForPending;
        }

        #endregion

        #region POST-requests
        /// <summary>
        /// Will lock this Event if it is not already locked. 
        /// </summary>
        /// <param name="lockDto">Contents should represent caller</param>
        [Route("Event/lock")]
        [HttpPost]
        public void Lock([FromBody] LockDto lockDto)
        {
            if (Logic.LockDto != null)
            {
                // There cannot be set a new lock, since a lock is already set.
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "Lock could not be acquired. Event is already locked."));
            }
            if (lockDto == null)
            {
                // Caller provided a null LockDto
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "Lock could not be set. An empty lock was provided."));
            }

            Logic.LockDto = lockDto;
        }

        /// <summary>
        /// Unlock will (attempt to) unlock this Event. May fail if Event is already locked
        /// </summary>
        /// <param name="eventAddressDto">Should represent caller</param>
        [Route("Event/lock")]
        [HttpDelete]
        public void Unlock([FromBody] EventAddressDto eventAddressDto)
        {
            // Check is made to see if caller is the same as the one, who locked the Event initially
            if(!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "Lock could be unlocked. Event was locked by someone else."));
            }

            Logic.LockDto = null;
        }
        #endregion
    }
}
