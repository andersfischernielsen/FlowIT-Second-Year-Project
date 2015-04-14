using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Interfaces;
using Event.Storage;

namespace Event.Controllers
{
    /// <summary>
    /// EventController handles requests regarding the Event as a whole; i.e. requests that manipulates all
    /// aspects of this Event. 
    /// </summary>
    public class EventController : ApiController
    {
        private readonly IEventLogic _logic;

        /// <summary>
        /// Constructor which is used by the WebAPI framework at runtime.
        /// </summary>
        public EventController()
        {
            _logic = new EventLogic(new EventStorage(new EventContext()));
        }

        /// <summary>
        /// Constructor used for dependency injection.
        /// </summary>
        /// <param name="logic">The logic to inject</param>
        public EventController(IEventLogic logic)
        {
            _logic = logic;
        }


        #region EventDto
        /// <summary>
        /// Get the entire Event, (namely rules and state for this Event)
        /// </summary>
        /// <param name="eventId">The id of the Event, that you wish to get an EventDto representation of</param>
        /// <returns>A task containing a single EventDto which represents the Events current state.</returns>
        [Route("events/{eventId}")]
        [HttpGet]
        public async Task<EventDto> GetEvent(string eventId)
        {
            _logic.EventId = eventId;
            // Check if provided eventId exists
            if (!_logic.EventIdExists())
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, String.Format("{0} event does not exist", eventId)));
            }

            // Dismiss request if Event is currently locked
            if (_logic.IsLocked())
            {
                // Event is currently locked)
                StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            return await _logic.EventDto;
        }

        /// <summary>
        /// Sets up an Event at this WebAPI
        /// </summary>
        /// <param name="eventDto">The data (ruleset and initial state), this Event should be set to</param>
        /// <returns></returns>
        [Route("events")]
        [HttpPost]
        public async Task PostEvent([FromBody] EventDto eventDto)
        {
            // Check that provided input can be mapped onto an instance of EventDto
            if (!ModelState.IsValid)
            {
                // For nicer debugging, a list of the conflicting mappings is provided
                var conflictElements = new StringBuilder();
                foreach (var key in ModelState.Keys)
                {
                    if (!ModelState.IsValidField(key))
                    {
                        conflictElements.Append(key + ", ");
                    }
                }
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto an instance of EventDto " +
                                                conflictElements));
            }

            if (eventDto == null)
            {
                // TODO: Provide nicer description / error message
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            _logic.EventId = eventDto.EventId;
            // Check for non-existing eventId
            if (_logic.EventIdExists())
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, String.Format("{0} event already exists", eventDto.EventId)));
            }

            // TODO: An Event that has just been posted, should not be able to be locked already...Delete! 
            // Dismiss request if Event is currently locked
            if (_logic.IsLocked())
            {
                // Event is currently locked)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Event is currently locked"));

            }

            // Prepare for method-call: Gets own URI (i.e. http://address)
            var s = string.Format("{0}://{1}", Request.RequestUri.Scheme, Request.RequestUri.Authority);
            var ownUri = new Uri(s);

            // Method call
            await _logic.InitializeEvent(eventDto, ownUri);
        }

        /// <summary>
        /// DeleteEvent will delete an Event
        /// </summary>
        /// <param name="eventId">The id of the Event to be deleted</param>
        /// <returns></returns>
        [Route("events/{eventId}")]
        [HttpDelete]
        public async Task DeleteEvent(string eventId)
        {
            _logic.EventId = eventId;
            // Dismiss request if Event is currently locked
            if (_logic.IsLocked())
            {
                // Event is currently locked
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed,
                    "Event is currently locked"));
            }
            await _logic.DeleteEvent();
        }

        #endregion

        #region Reset Event

        /// <summary>
        /// This method resets an Event. Note, that this will reset the three bool-values of the Event
        /// to their initial values, and reset any locks!. 
        /// </summary>
        /// <param name="eventId">Id of the Event, that is to be reset</param>
        /// <param name="eventDto">Empty container</param>
        /// <returns></returns>
        [Route("events/{eventId}/reset")]
        [HttpPut]
        public void ResetEvent([FromBody] EventDto eventDto, string eventId)
        {
            using (IResetLogic resetLogic = new ResetLogic(eventId))
            {
                // Reset locks
                resetLogic.UnlockEvent();

                // Reset to initial values
                resetLogic.ResetToInitialValues();
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            // TODO: Breakpoint inserted to check that dispose is called.
            base.Dispose(disposing);
        }
    }
}