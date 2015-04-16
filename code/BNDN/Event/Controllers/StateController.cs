using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Exceptions;
using Event.Interfaces;
using Event.Logic;

namespace Event.Controllers
{
    public class StateController : ApiController
    {
        private readonly IStateLogic _logic;
        /// <summary>
        /// Runtime Constructor of StateController.
        /// Uses default implementation of IStateLogic and dependencies.
        /// </summary>
        public StateController()
        {
            
            _logic = new StateLogic();
        }

        /// <summary>
        /// Constructor used for Dependency injection.
        /// </summary>
        /// <param name="logic">An implementation of IStateLogic.</param>
        public StateController(IStateLogic logic)
        {
            _logic = logic;
        }

        /// <summary>
        /// GetExecuted returns the Event's current (bool) Executed value. 
        /// </summary>
        /// <param name="senderId">Content should represent the caller of this method</param>
        /// <param name="eventId">Id of the Event, whose Executed value should be returned</param>
        /// <returns>Event's current Executed value</returns>
        [Route("events/{eventId}/executed/{senderId}")]
        [HttpGet]
        public async Task<bool> GetExecuted(string eventId, string senderId)
        {
            try
            {
                return await _logic.IsExecuted(eventId, senderId);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Event is locked"));
            }
        }

        /// <summary>
        /// GetIncluded returns Event's current value for Included (bool). 
        /// </summary>
        /// <param name="senderId">Content should represent caller of the method.</param>
        /// <param name="eventId">The id of the Event, whose Included value is to be returned</param>
        /// <returns>Current value of Event's (bool) Included value</returns>
        [Route("events/{eventId}/included/{senderId}")]
        [HttpGet]
        public async Task<bool> GetIncluded(string senderId, string eventId)
        {
            try
            {
                return await _logic.IsIncluded(eventId, senderId);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Event is locked"));
            }
        }

        /// <summary>
        /// Returns the current state of the events.
        /// </summary>
        /// <param name="senderId">Content of this should represent caller</param>
        /// <param name="eventId">The id of the Event, whose StateDto is to be returned</param>
        /// <returns>A Task resulting in an EventStateDto which contains 3 
        /// booleans with the current state of the Event, plus a 4th boolean 
        /// which states whether the Event is currently executable</returns>
        [Route("events/{eventId}/state/{senderId}")]
        [HttpGet]
        public async Task<EventStateDto> GetState(string eventId, string senderId)
        {
            try
            {
                return await _logic.GetStateDto(eventId, senderId);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Event is locked"));
            }
        }

        /// <summary>
        /// Updates Event's current (bool) value for Included
        /// </summary>
        /// <param name="eventAddressDto">Content should represent caller. Used to identify caller.</param>
        /// <param name="boolValueForIncluded">The value that Included should be set to</param>
        /// <param name="eventId">The id of the Event, whose Included value is to be updated</param>
        [Route("events/{eventId}/included/{boolValueForIncluded}")]
        [HttpPut]
        public async Task UpdateIncluded(string eventId, bool boolValueForIncluded, [FromBody] EventAddressDto eventAddressDto)
        {
            // Check if provided input can be mapped onto an instance of EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of EventAddressDto"));
            }
            try
            {
                await _logic.SetIncluded(eventId, eventAddressDto.Id, boolValueForIncluded);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Event is locked"));
            }
            // TODO: Research what the right response to a PUT call is (I believe it is the updates value of the property) 
            // TODO: (and implement it here and on the other PUT-calls)
        }

        /// <summary>
        /// Updates Event's current (bool) value for Pending
        /// </summary>
        /// <param name="eventAddressDto">Content should represent caller.</param>
        /// <param name="boolValueForPending">The value Pending should be set to</param>
        /// <param name="eventId">The id of the Event, whose Pending value is to be set</param>
        [Route("events/{eventId}/pending/{boolValueForPending}")]
        [HttpPut]
        public async Task UpdatePending(string eventId, bool boolValueForPending, [FromBody] EventAddressDto eventAddressDto)
        {
            // Check to see whether caller provided a legal instance of an EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of EventAddressDto"));
            }
            try
            {
                await _logic.SetPending(eventId, eventAddressDto.Id, boolValueForPending);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Event is locked"));
            }
            
        }

        /// <summary>
        /// Executes this event. Only Clients should invoke this.
        /// </summary>
        /// <param name="executeDto">An executeDto with the roles of the given user wishing to execute.</param>
        /// <param name="eventId">The id of the Event, who is to be executed</param>
        /// <returns></returns>
        [Route("events/{eventId}/executed")]
        [HttpPut]
        public async Task<bool> Execute([FromBody] RoleDto executeDto, string eventId)
        {
            // Check that provided input can be mapped onto an instance of ExecuteDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of ExecuteDto; " +
                    "No roles was provided"));
            }
            try
            {
                return await _logic.Execute(eventId, executeDto);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Event is locked"));
            }
            catch (NotAuthorizedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "You do not have permission to execute this event"));
            }
            catch (NotExecutableException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed,
                    "Event is not executable."));
            }
            catch (FailedToLockOtherEventException)
            {
                return false;
            }
            catch (FailedToUnlockOtherEventException)
            {
                return false;
            }
            catch (FailedToUpdateStateException)
            {
                return false;
            }
            catch (FailedToUpdateStateAtOtherEventException)
            {
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            base.Dispose(disposing);
        }
    }
}
