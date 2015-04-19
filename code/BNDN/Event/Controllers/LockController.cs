using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Event.Communicators;
using Event.Exceptions;
using Event.Interfaces;
using Event.Logic;
using Event.Storage;

namespace Event.Controllers
{
    public class LockController : ApiController
    {
        private readonly ILockingLogic _lockLogic;

        // Default controller used by framework
        public LockController()
        {
            _lockLogic = new LockingLogic(new EventStorage(new EventContext()), new EventCommunicator());
        }

        // Controller used to dependency-inject during testing
        public LockController(ILockingLogic lockLogic)
        {
            _lockLogic = lockLogic;
        }

        /// <summary>
        /// Will lock this Event if it is not already locked.
        /// This POST call should be received either a) from the Event itself (when it is about to execute) or when
        /// b) another Event (that has this Event in it's dependencies) asks it to lock.
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="lockDto">Contents should represent caller</param>
        /// <param name="eventId">The id of the Event, that caller wants to lock</param>
        [Route("events/{workflowId}/{eventId}/lock")]
        [HttpPost]
        public void Lock(string workflowId, string eventId, [FromBody] LockDto lockDto)
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
            try
            {
                _lockLogic.LockSelf(workflowId, eventId, lockDto);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        /// <summary>
        /// Unlock will (attempt to) unlock this Event. May fail if Event is already locked
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="senderId">Should represent caller</param>
        /// <param name="eventId">The id of the Event, that caller seeks to unlock</param>
        [Route("Events/{eventId}/lock/{senderId}")]
        [HttpDelete]
        public void Unlock(string workflowId, string eventId, string senderId)
        {
            try
            {
                _lockLogic.UnlockSelf(workflowId, eventId, senderId);
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Could not unlock: One or more of the provided arguments was null"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                    "Could not unlock: Event is locked by someone else"));
            }
        }
    }
}
