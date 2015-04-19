using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Event.Communicators;
using Event.Interfaces;
using Event.Logic;
using Event.Storage;

namespace Event.Controllers
{
    public class LockController : ApiController
    {
        private readonly ILockingLogic _lockLogic;
        private readonly IEventStorage _storage;

        // Default controller used by framework
        public LockController()
        {
            _storage = new EventStorage(new EventContext());
            _lockLogic = new LockingLogic(_storage, new EventCommunicator());
        }

        // Controller used to dependency-inject during testing
        public LockController(ILockingLogic lockLogic, IEventStorage storage)
        {
            _lockLogic = lockLogic;
            _storage = storage;
        }

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
                _lockLogic.LockSelf(eventId, lockDto);
            }
            catch (Exception)
            {
                
                throw;
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
            try
            {
                _lockLogic.UnlockSelf(eventId, senderId);
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
