using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Exceptions;
using Event.Exceptions;
using Event.Exceptions.EventInteraction;
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
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="senderId">Content should represent the caller of this method</param>
        /// <param name="eventId">Id of the Event, whose Executed value should be returned</param>
        /// <returns>Event's current Executed value</returns>
        [Route("events/{workflowId}/{eventId}/executed/{senderId}")]
        [HttpGet]
        public async Task<bool> GetExecuted(string workflowId, string eventId, string senderId)
        {
            try
            {
                return await _logic.IsExecuted(workflowId, eventId, senderId);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "GetExecuted: Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "GetExecuted: Event is locked"));
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "GetExecuted: Seems input was not satisfactory"));
            }
        }

        /// <summary>
        /// GetIncluded returns Event's current value for Included (bool). 
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="senderId">Content should represent caller of the method.</param>
        /// <param name="eventId">The id of the Event, whose Included value is to be returned</param>
        /// <returns>Current value of Event's (bool) Included value</returns>
        [Route("events/{workflowId}/{eventId}/included/{senderId}")]
        [HttpGet]
        public async Task<bool> GetIncluded(string workflowId, string senderId, string eventId)
        {
            try
            {
                return await _logic.IsIncluded(workflowId, eventId, senderId);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "GetIncluded: Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "GetIncluded: Event is locked"));
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "GetIncluded: Seems input was not satisfactory"));
            }
        }

        /// <summary>
        /// Returns the current state of the event.
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="senderId">Content of this should represent caller</param>
        /// <param name="eventId">The id of the Event, whose StateDto is to be returned</param>
        /// <returns>A Task resulting in an EventStateDto which contains 3 
        /// booleans with the current state of the Event, plus a 4th boolean 
        /// which states whether the Event is currently executable</returns>
        [Route("events/{workflowId}/{eventId}/state/{senderId}")]
        [HttpGet]
        public async Task<EventStateDto> GetState(string workflowId, string eventId, string senderId)
        {
            try
            {
                return await _logic.GetStateDto(workflowId, eventId, senderId);
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "GetState: Seems input was not satisfactory"));
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "GetState: Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "GetState: Event is locked"));
            }
        }

        /// <summary>
        /// Updates Event's current (bool) value for Included
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventAddressDto">Content should represent caller. Used to identify caller.</param>
        /// <param name="boolValueForIncluded">The value that Included should be set to</param>
        /// <param name="eventId">The id of the Event, whose Included value is to be updated</param>
        [Route("events/{workflowId}/{eventId}/included/{boolValueForIncluded}")]
        [HttpPut]
        public async Task UpdateIncluded(string workflowId, string eventId, bool boolValueForIncluded, [FromBody] EventAddressDto eventAddressDto)
        {
            // Check if provided input can be mapped onto an instance of EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "UpdateIncluded: Provided input could not be mapped onto an instance of EventAddressDto"));
            }
            try
            {
                await _logic.SetIncluded(workflowId, eventId, eventAddressDto.Id, boolValueForIncluded);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "UpdateIncluded: Not Found"));
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "UpdateIncluded: Provided input was null"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "UpdateIncluded: Event is locked"));
            }
            catch (FailedToUpdateIncludedAtAnotherEventException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "UpdateIncluded: Failed to get Included from another Event"));
            }
        }

        /// <summary>
        /// Updates Event's current (bool) value for Pending
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventAddressDto">Content should represent caller.</param>
        /// <param name="boolValueForPending">The value Pending should be set to</param>
        /// <param name="eventId">The id of the Event, whose Pending value is to be set</param>
        [Route("events/{workflowId}/{eventId}/pending/{boolValueForPending}")]
        [HttpPut]
        public async Task UpdatePending(string workflowId, string eventId, bool boolValueForPending, [FromBody] EventAddressDto eventAddressDto)
        {
            // Check to see whether caller provided a legal instance of an EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "UpdatePending: Provided input could not be mapped onto an instance of EventAddressDto"));
            }
            try
            {
                await _logic.SetPending(workflowId, eventId, eventAddressDto.Id, boolValueForPending);
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "UpdatePending: Provided input was null"));
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "UpdatePending: Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "UpdatePending: Event is locked"));
            }
            catch (FailedToUpdatePendingAtAnotherEventException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "UpdatePending: Failed to update Pending at another Event"));
            }

        }

        /// <summary>
        /// Executes this event. Only Clients should invoke this.
        /// </summary>
        /// <param name="executeDto">An executeDto with the roles of the given user wishing to execute.</param>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventId">The id of the Event, who is to be executed</param>
        /// <returns></returns>
        [Route("events/{workflowId}/{eventId}/executed")]
        [HttpPut]
        public async Task<bool> Execute(string workflowId, string eventId, [FromBody] RoleDto executeDto)
        {
            // Check that provided input can be mapped onto an instance of ExecuteDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Execute: Provided input could not be mapped onto an instance of ExecuteDto; " +
                    "No roles was provided"));
            }
            try
            {
                return await _logic.Execute(workflowId, eventId, executeDto);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "Execute: Not Found"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                    "Execute: Event is locked"));
            }
            catch (NotAuthorizedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "Execute: You do not have permission to execute this event"));
            }
            catch (NotExecutableException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed,
                    "Execute: Event is not executable."));
            }
            catch (FailedToLockOtherEventException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                    "Execute: Another event is locked"));
            }
            catch (FailedToUnlockOtherEventException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "Execute: Could not unlock other events."));
            }
            catch (FailedToUpdateStateException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "Execute: Failed to update state internally."));
            }
            catch (FailedToUpdateStateAtOtherEventException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, 
                    "Execute: Failed to update state at another dependent Event"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            base.Dispose(disposing);
        }
    }
}
