﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.DTO.Event;
using Common.DTO.Shared;
using Common.Exceptions;
using Event.Exceptions;
using Event.Exceptions.EventInteraction;
using Event.Interfaces;
using Event.Logic;

namespace Event.Controllers
{
    /// <summary>
    /// StateController handles HTTP-requests regarding State on an Event. 
    /// </summary>
    public class StateController : ApiController
    {
        private readonly IStateLogic _logic;
        private readonly IEventHistoryLogic _historyLogic;

        /// <summary>
        /// Runtime Constructor of StateController.
        /// Uses default implementation of IStateLogic and dependencies.
        /// </summary>
        public StateController()
        {
            _logic = new StateLogic();
            _historyLogic = new EventHistoryLogic();
        }

        /// <summary>
        /// Constructor used for Dependency injection.
        /// </summary>
        /// <param name="logic">An implementation of IStateLogic.</param>
        public StateController(IStateLogic logic, IEventHistoryLogic historyLogic)
        {
            _logic = logic;
            _historyLogic = historyLogic;
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
                var toReturn = await _logic.IsExecuted(workflowId, eventId, senderId);
                await _historyLogic.SaveSuccesfullCall("GET", "GetExecuted", eventId, workflowId);
                return toReturn;
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "GET", "GetExecuted", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not Found"));
            }
            catch (LockedException e)
            {
                _historyLogic.SaveException(e, "GET", "GetExecuted", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Event is locked"));
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "GET", "GetExecuted", eventId, workflowId);

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
                var toReturn = await _logic.IsIncluded(workflowId, eventId, senderId);
                await _historyLogic.SaveSuccesfullCall("GET", "GetIncluded", eventId, workflowId);

                return toReturn;
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "GET", "GetExecuted", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "GetIncluded: Not Found"));
            }
            catch (LockedException e)
            {
                _historyLogic.SaveException(e, "GET", "GetExecuted", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                        "GetIncluded: Event is locked"));
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "GET", "GetExecuted", eventId, workflowId);

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
                var toReturn = await _logic.GetStateDto(workflowId, eventId, senderId);
                await _historyLogic.SaveSuccesfullCall("GET", "GetState", eventId, workflowId);

                return toReturn;
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "GET", "GetState", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "GetState: Seems input was not satisfactory"));
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "GET", "GetState", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "GetState: Not Found"));
            }
            catch (LockedException e)
            {
                _historyLogic.SaveException(e, "GET", "GetState", eventId, workflowId);

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
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "UpdateIncluded: Provided input could not be mapped onto an instance of EventAddressDto"));
                await _historyLogic.SaveException(toThrow, "PUT", "UpdateIncluded", eventId, workflowId);
                throw toThrow;
            }

            try
            {
                await _logic.SetIncluded(workflowId, eventId, eventAddressDto.Id, boolValueForIncluded);
                await _historyLogic.SaveSuccesfullCall("PUT", "UpdateIncluded", eventId, workflowId);
                return;
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "PUT", "UpdateIncluded", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "UpdateIncluded: Not Found"));
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "PUT", "UpdateIncluded", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "UpdateIncluded: Provided input was null"));
            }
            catch (LockedException e)
            {
                _historyLogic.SaveException(e, "PUT", "UpdateIncluded", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "UpdateIncluded: Event is locked"));
            }
            catch (FailedToUpdateIncludedAtAnotherEventException e)
            {
                _historyLogic.SaveException(e, "PUT", "UpdateIncluded", eventId, workflowId);

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
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "UpdatePending: Provided input could not be mapped onto an instance of EventAddressDto"));
                await _historyLogic.SaveException(toThrow, "PUT", "UpdatePending", eventId, workflowId);
                throw toThrow;
            }

            try
            {
                await _logic.SetPending(workflowId, eventId, eventAddressDto.Id, boolValueForPending);
                await _historyLogic.SaveSuccesfullCall("PUT", "UpdatePending", eventId, workflowId);
                return;
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "PUT", "UpdatePending", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "UpdatePending: Provided input was null"));
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "PUT", "UpdatePending", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "UpdatePending: Not Found"));
            }
            catch (LockedException e)
            {
                _historyLogic.SaveException(e, "PUT", "UpdatePending", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "UpdatePending: Event is locked"));
            }
            catch (FailedToUpdatePendingAtAnotherEventException e)
            {
                _historyLogic.SaveException(e, "PUT", "UpdatePending", eventId, workflowId);

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
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of ExecuteDto; " +
                    "No roles was provided"));
                await _historyLogic.SaveException(toThrow, "PUT", "Execute", eventId, workflowId);

                throw toThrow;
            }
            try
            {
                var toReturn = await _logic.Execute(workflowId, eventId, executeDto);   
                await _historyLogic.SaveSuccesfullCall("PUT", "Execute", eventId, workflowId);

                return toReturn;
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "PUT", "Execute", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "Execute: Not Found"));
            }
            catch (LockedException e)
            {
                _historyLogic.SaveException(e, "PUT", "Execute", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Event is locked"));
            }
            catch (UnauthorizedException e)
            {
                _historyLogic.SaveException(e, "PUT", "Execute", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "You do not have permission to execute this event"));
            }
            catch (NotExecutableException e)
            {
                _historyLogic.SaveException(e, "PUT", "Execute", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed,
                    "Event is not executable."));
            }
            catch (FailedToLockOtherEventException e)
            {
                _historyLogic.SaveException(e, "PUT", "Execute", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict, "Another event is locked"));
            }
            catch (FailedToUnlockOtherEventException e)
            {
                _historyLogic.SaveException(e, "PUT", "Execute", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Could not unlock other events."));
            }
            catch (FailedToUpdateStateException e)
            {
                _historyLogic.SaveException(e, "PUT", "Execute", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "State could not be saved!"));
            }
            catch (FailedToUpdateStateAtOtherEventException e)
            {
                _historyLogic.SaveException(e, "PUT", "Execute", eventId, workflowId);

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Another event could not save state!"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            _historyLogic.Dispose();
            base.Dispose(disposing);
        }
    }
}
