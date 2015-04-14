using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using Common;
using Event.Interfaces;
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
        private readonly IEventLogic _logic;

        public EventStateController()
        {
            _logic = new EventLogic(new EventStorage(new EventContext()));
        }

        public EventStateController(IEventLogic logic)
        {
            _logic = logic;
        }

        #region GET-requests

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
            _logic.EventId = eventId;
            // Check is made to see if caller is allowed to execute this method at the moment. 
            if (!_logic.CallerIsAllowedToOperate(senderId))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Cannot access this property. The event is locked."));
            }
            return _logic.Executed;
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
            _logic.EventId = eventId;
            // Check is made to see if caller is allowed to execute this method
            if (!_logic.CallerIsAllowedToOperate(senderId))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Cannot access this property. The event is locked."));
            }
            return _logic.Included;
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
            _logic.EventId = eventId;
            //Todo: The client uses this method and sends -1 as an ID. This is a bad solution, so refactoring is encouraged.
            // Check is made to see whether caller is allowed to execute this method at the moment
            if (!senderId.Equals("-1") && !_logic.CallerIsAllowedToOperate(senderId))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Cannot access this property. The event is locked."));
            }
            return await _logic.GetEventStateDto();
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
        public async Task<bool> Execute([FromBody] ExecuteDto executeDto, string eventId)
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
                _logic.Execute();
            }
            catch (Exception)
            {
                
                throw;
            }

            _logic.EventId = eventId;
            // Check that caller claims the right role for executing this Event
            if (!_logic.ProvidedRolesHasMatchWithEventRoles(executeDto.Roles))
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        "You do not have the correct role for executing this event."));
            }

            // Check if Event is currently locked
            if (_logic.IsLocked())
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        "Event is currently locked"));
            }
            // Check whether Event can be executed at the moment
            if (!(await _logic.IsExecutable()))
            {
                throw new HttpResponseException(
                    Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        "Event is not currently executable."));
            }

            // Lock all dependent Events (including one-self)
            // TODO: Check: Does the following include locking on this Event itself...?
            ILockLogic lockLogic = new LockLogic(_logic);
            if (await lockLogic.LockAll())
            {
                var allOk = true;
                try
                {
                    await _logic.Execute();
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
                    return true;
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
            _logic.EventId = eventId;
            // Check if provided input can be mapped onto an instance of EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of EventAddressDto"));
            }

            // Check to see if caller is currently allowed to execute this method
            if (!_logic.CallerIsAllowedToOperate(eventAddressDto.Id))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "This event is already locked by someone else."));
            }
            _logic.Included = boolValueForIncluded;

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
        public void UpdatePending([FromBody] EventAddressDto eventAddressDto, bool boolValueForPending, string eventId)
        {
            _logic.EventId = eventId;
            // Check to see whether caller provided a legal instance of an EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of EventAddressDto"));
            }

            // Check if caller is allowed to execute this method at the moment
            if (!_logic.CallerIsAllowedToOperate(eventAddressDto.Id))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "This event is already locked by someone else."));
            }
            _logic.Pending = boolValueForPending;
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
            _logic.EventId = eventId;
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

            if (_logic.IsLocked())
            {
                // Todo: Consider fixing this "hack"
                if (!_logic.CallerIsAllowedToOperate(lockDto.LockOwner))
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
            _logic.LockDto = lockDto;
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
            _logic.EventId = eventId;
            // Check is made to see if caller is the same as the one, who locked the Event initially
            // the CallerIsAllowedToOperate works on Id not Uri.
            if (!_logic.CallerIsAllowedToOperate(senderId))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Lock could not be unlocked. Event was locked by someone else."));
            }

            _logic.UnlockEvent();
        }
        #endregion
    }
}
