using System;
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
        [Route("event/pending/{id}")]
        [HttpGet]
        public bool GetPending(string id)
        {
            // Check is made to see if the caller is allowed to execute this method at the moment
            if (!Logic.CallerIsAllowedToOperate(new EventAddressDto() { Id = id }))
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
        [Route("event/executed/{id}")]
        [HttpGet]
        public bool GetExecuted(string id)
        {
            // Check is made to see if caller is allowed to execute this method at the moment. 
            if (!Logic.CallerIsAllowedToOperate(new EventAddressDto() { Id = id }))
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
        [Route("event/included/{id}")]
        [HttpGet]
        public bool GetIncluded(string id)
        {
            // Check is made to see if caller is allowed to execute this method
            if (!Logic.CallerIsAllowedToOperate(new EventAddressDto() { Id = id }))
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
        [Route("event/executable/{id}")]
        [HttpGet]
        public async Task<bool> GetExecutable(string id)
        {
            // Check is made to see if caller is allowed to execute this method (at the moment)
            if (!Logic.CallerIsAllowedToOperate(new EventAddressDto() { Id = id }))
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
        [Route("event/state/{id}")]
        [HttpGet]
        public async Task<EventStateDto> GetState(string id)
        {
            //Todo: The client uses this method and sends -1 as an ID. This is a bad solution, so refactoring is encouraged.
            // Check is made to see whether caller is allowed to execute this method at the moment
            if (!id.Equals("-1") && !Logic.CallerIsAllowedToOperate(new EventAddressDto() { Id = id }))
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
        /// <param name="executeDto">An executeDto with the roles of the given user wishing to execute.</param>
        /// <returns></returns>
        [Route("event/executed")]
        [HttpPut]
        public async Task Execute([FromBody] ExecuteDto executeDto)
        {
            // Check that provided input can be mapped onto an instance of ExecuteDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto an instance of ExecuteDto; " +
                                                "No roles was provided"));
            }

            // Check that caller claims the right role for executing this Event
            if (!executeDto.Roles.Contains(Logic.Role))
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        "You do not have the correct role for executing this event."));
            }

            // Check if Event is currently locked
            if (Logic.IsLocked())
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        "Event is currently locked"));
            }
            // Check whether Event can be executed at the moment
            if (!(await Logic.IsExecutable()))
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        "Event is not currently executable."));
            }

            // Lock all dependent Events (including one-self)
            // TODO: Check: Does the following include locking on this Event itself...?
            LockLogic lockLogic = new LockLogic();
            if (await lockLogic.LockAll())
            {
                var allOk = true;
                try
                {
                    await Logic.Execute();
                }
                catch (Exception)
                {
                    allOk = false;
                }


                if (!await lockLogic.UnlockAll())
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Failed at unlocking all the locked events."));
                    //Kunne ikke unlocke alt, hvad skal der ske?
                }
                if (allOk)
                {
                    // TODO: Discuss is this really the way (i.e. through an Exception) to signal that Event was executed?
                    // TODO: Is there no other (nicer) alternative to raising an exception? 
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.OK, true));
                }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Failed at updating other events."));
                }
            }
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Another transaction is going on, try again later"));
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
            // Check if provided input can be mapped onto an instance of EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                "Provided input could not be mapped onto an instance of EventAddressDto"));
            }

            // Check to see if caller is currently allowed to execute this method
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "This event is already locked by someone else."));
            }
            Logic.Included = boolValueForIncluded;

            // TODO: Research what the right response to a PUT call is (I believe it is the updates value of the property) 
            // TODO: (and implement it here and on the other PUT-calls)
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
            // Check to see whether caller provided a legal instance of an EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                "Provided input could not be mapped onto an instance of EventAddressDto"));
            }

            // Check if caller is allowed to execute this method at the moment
            if (!Logic.CallerIsAllowedToOperate(eventAddressDto))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "This event is already locked by someone else."));
            }
            Logic.Pending = boolValueForPending;
        }

        #endregion

        #region Lock-related
        /// <summary>
        /// Will lock this Event if it is not already locked.
        /// This POST call should be received either a) from the Event itself (when it is about to execute) or when
        /// b) another Event (that has this Event in it's dependencies) asks it to lock.
        /// </summary>
        /// <param name="lockDto">Contents should represent caller</param>
        [Route("Event/lock")]
        [HttpPost]
        public void Lock([FromBody] LockDto lockDto)
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

            if (Logic.IsLocked())
            {
                // Todo: Consider fixing this "hack"
                if (!Logic.CallerIsAllowedToOperate(new EventAddressDto { Id = lockDto.LockOwner }))
                {
                    // There cannot be set a new lock, since a lock is already set.
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Lock could not be acquired. Event is already locked."));
                }
            }

            // Checks that the provided lockDto actually has sensible values for its fields.
            // TODO: Consider refactoring this logic into other (logic-handling class) class
            if (String.IsNullOrEmpty(lockDto.LockOwner) || String.IsNullOrWhiteSpace(lockDto.LockOwner))
            {
                // Reject request on setting the lockDto
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Logic.LockDto = lockDto;
        }

        /// <summary>
        /// Unlock will (attempt to) unlock this Event. May fail if Event is already locked
        /// </summary>
        /// <param name="id">Should represent caller</param>
        [Route("Event/lock/{id}")]
        [HttpDelete]
        public void Unlock(string id)
        {
            // Check is made to see if caller is the same as the one, who locked the Event initially
            // the CallerIsAllowedToOperate works on Id not Uri.
            if (!Logic.CallerIsAllowedToOperate(new EventAddressDto() { Id = id }))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Lock could not be unlocked. Event was locked by someone else."));
            }

            // TODO: Consider having a "ClearLock"-"Unlock" method that sets LockDto to null <- done! 
            //Logic.LockDto = null;
            Logic.UnlockEvent();
        }
        #endregion
    }
}
