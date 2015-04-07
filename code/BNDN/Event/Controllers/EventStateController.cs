using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using Common;
using Event.Storage;

namespace Event.Controllers
{
    /// <summary>
    /// EventStateController handles manipulations on Event't State, including locks on Event.
    /// The reason why we're not using the same EventAddressDtos at GET-requests as on PUT-requests
    /// is a GET-request should not contain something; only get something.
    /// </summary>
    public class EventStateController : ApiController
    {

        #region GET-requests
        /// <summary>
        /// GetPending returns the (bool) value of Event's pending state. Callers of this method must identify themselves
        /// through an EventAddressDto
        /// <param name="senderId">Used as a representation for caller of this method</param>
        /// <param name="eventId">The id of the Event, whose pending value is to be returned</param>
        /// <returns>Event's pending (bool) value</returns>
        /// </summary>
        [Route("events/{eventId}/pending/{senderId}")]
        [HttpGet]
        public bool GetPending(string senderId, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Check is made to see if the caller is allowed to execute this method at the moment
                if (!logic.CallerIsAllowedToOperate(senderId))
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Cannot access this property. The event is locked."));
                }
                return logic.Pending;
            }
        }

        /// <summary>
        /// GetExecuted returns the Event's current (bool) Executed value. 
        /// </summary>
        /// <param name="senderId">Content should represent the caller of this method</param>
        /// <param name="eventId">Id of the Event, whose Executed value should be returned</param>
        /// <returns>Event's current Executed value</returns>
        [Route("events/{eventId}/executed/{senderId}")]
        [HttpGet]
        public bool GetExecuted(string senderId, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Check is made to see if caller is allowed to execute this method at the moment. 
                if (!logic.CallerIsAllowedToOperate(senderId))
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Cannot access this property. The event is locked."));
                }
                return logic.Executed;
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
        public bool GetIncluded(string senderId, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Check is made to see if caller is allowed to execute this method
                if (!logic.CallerIsAllowedToOperate(senderId))
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Cannot access this property. The event is locked."));
                }
                return logic.Included;
            }
        }

        /// <summary>
        /// Returns Event's current value of Executable.
        /// </summary>
        /// <param name="senderId">Content should represent caller</param>
        /// <param name="eventId">Id of the Event, whose Executable value is to be returned</param>
        /// <returns>Current value of Event's Executable</returns>
        [Route("events/{eventId}/executable/{senderId}")]
        [HttpGet]
        public async Task<bool> GetExecutable(string senderId, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Check is made to see if caller is allowed to execute this method (at the moment)
                if (!logic.CallerIsAllowedToOperate(senderId))
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Cannot access this property. The event is locked."));
                }
                return await logic.IsExecutable();
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
        public async Task<EventStateDto> GetState(string senderId, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                //Todo: The client uses this method and sends -1 as an ID. This is a bad solution, so refactoring is encouraged.
                // Check is made to see whether caller is allowed to execute this method at the moment
                if (!senderId.Equals("-1") && !logic.CallerIsAllowedToOperate(senderId))
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Cannot access this property. The event is locked."));
                }
                return await logic.EventStateDto;
            }
        }
        #endregion

        // TODO: We need to decide on a consistent way of updating these values; a) the (bool) value in body or b) the bool value in the url...
        #region PUT-requests

        /// <summary>
        /// Executes this event. Only Clients should invoke this.
        /// todo: Should be able to return something to the caller.
        /// </summary>
        /// <param name="executeDto">An executeDto with the roles of the given user wishing to execute.</param>
        /// <param name="eventId">The id of the Event, who is to be executed</param>
        /// <returns></returns>
        [Route("events/{eventId}/executed")]
        [HttpPut]
        public async Task Execute([FromBody] ExecuteDto executeDto, string eventId)
        {
            // Check that provided input can be mapped onto an instance of ExecuteDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of ExecuteDto; " +
                    "No roles was provided"));
            }

            using (var logic = new EventLogic(eventId))
            {
                // Check that caller claims the right role for executing this Event
                if (!logic.ProvidedRolesHasMatchWithEventRoles(executeDto.Roles))
                {
                    throw new HttpResponseException(
                        Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            "You do not have the correct role for executing this event."));
                }

                // Check if Event is currently locked
                if (logic.IsLocked())
                {
                    throw new HttpResponseException(
                        Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            "Event is currently locked"));
                }
                // Check whether Event can be executed at the moment
                if (!(await logic.IsExecutable()))
                {
                    throw new HttpResponseException(
                        Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            "Event is not currently executable."));
                }

                // Lock all dependent Events (including one-self)
                // TODO: Check: Does the following include locking on this Event itself...?
                LockLogic lockLogic = new LockLogic(logic);
                if (await lockLogic.LockAll())
                {
                    var allOk = true;
                    try
                    {
                        await logic.Execute();
                    }
                    catch (Exception)
                    {
                        allOk = false;
                    }

                    try
                    {
                        if (!await lockLogic.UnlockAll())
                        {
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                "Failed at unlocking all the locked events."));
                            //Kunne ikke unlocke alt, hvad skal der ske?
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.StackTrace);
                    }
                    if (allOk)
                    {
                        // TODO: Discuss is this really the way (i.e. through an Exception) to signal that Event was executed?
                        // TODO: Is there no other (nicer) alternative to raising an exception? 
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.OK, true));
                    }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            "Failed at updating other events."));
                    }
                }
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Another transaction is going on, try again later"));
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
        public void UpdateIncluded([FromBody] EventAddressDto eventAddressDto, bool boolValueForIncluded, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Check if provided input can be mapped onto an instance of EventAddressDto
                if (!ModelState.IsValid)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Provided input could not be mapped onto an instance of EventAddressDto"));
                }

                // Check to see if caller is currently allowed to execute this method
                if (!logic.CallerIsAllowedToOperate(eventAddressDto.Id))
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "This event is already locked by someone else."));
                }
                logic.Included = boolValueForIncluded;

                // TODO: Research what the right response to a PUT call is (I believe it is the updates value of the property) 
                // TODO: (and implement it here and on the other PUT-calls)
            }
        }

        /// <summary>
        /// Updates Event's current (bool) value for Pending
        /// </summary>
        /// <param name="eventAddressDto">Content should represent caller.</param>
        /// <param name="boolValueForPending">The value Pending should be set to</param>
        /// <param name="eventId">The id of the Event, whose Pending value is to be set</param>
        [Route("events/{eventId}/pending/{boolValueForPending}")]
        [HttpPut]
        public void UpdatePending([FromBody] EventAddressDto eventAddressDto, bool boolValueForPending, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Check to see whether caller provided a legal instance of an EventAddressDto
                if (!ModelState.IsValid)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Provided input could not be mapped onto an instance of EventAddressDto"));
                }

                // Check if caller is allowed to execute this method at the moment
                if (!logic.CallerIsAllowedToOperate(eventAddressDto.Id))
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "This event is already locked by someone else."));
                }
                logic.Pending = boolValueForPending;
            }
        }

        #endregion

        #region Lock-related
        /// <summary>
        /// Will lock this Event if it is not already locked.
        /// This POST call should be received either a) from the Event itself (when it is about to execute) or when
        /// b) another Event (that has this Event in it's dependencies) asks it to lock.
        /// </summary>
        /// <param name="lockDto">Contents should represent caller</param>
        /// <param name="eventId">The id of the Event, that caller wants to lock</param>
        [Route("Events/{eventId}/lock")]
        [HttpPost]
        public void Lock([FromBody] LockDto lockDto, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                if (!ModelState.IsValid)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Provided input could not be mapped onto an instance of LockDto"));
                }

                if (lockDto == null)
                {
                    // TODO: Discuss: With the above !ModelState.IsValid check this check should not necessary. Can we remove? 
                    // Caller provided a null LockDto
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Lock could not be set. An empty (null) lock was provided. If your intent" +
                        " is to unlock the Event issue a DELETE request on  event/lock instead."));
                }

                if (logic.IsLocked())
                {
                    // Todo: Consider fixing this "hack"
                    if (!logic.CallerIsAllowedToOperate(lockDto.LockOwner))
                    {
                        // There cannot be set a new lock, since a lock is already set.
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            "Lock could not be acquired. Event is already locked."));
                    }
                    return;
                }

                // Checks that the provided lockDto actually has sensible values for its fields.
                // TODO: Consider refactoring this logic into other (logic-handling class) class
                if (String.IsNullOrEmpty(lockDto.LockOwner) || String.IsNullOrWhiteSpace(lockDto.LockOwner))
                {
                    // Reject request on setting the lockDto
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                lockDto.Id = eventId;
                logic.LockDto = lockDto;
            }
        }

        /// <summary>
        /// Unlock will (attempt to) unlock this Event. May fail if Event is already locked
        /// </summary>
        /// <param name="senderId">Should represent caller</param>
        /// <param name="eventId">The id of the Event, that caller seeks to unlock</param>
        [Route("Events/{eventId}/lock/{senderId}")]
        [HttpDelete]
        public void Unlock(string senderId, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Check is made to see if caller is the same as the one, who locked the Event initially
                // the CallerIsAllowedToOperate works on Id not Uri.
                if (!logic.CallerIsAllowedToOperate(senderId))
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Lock could not be unlocked. Event was locked by someone else."));
                }

                logic.UnlockEvent();
            }
        }
        #endregion
    }
}
