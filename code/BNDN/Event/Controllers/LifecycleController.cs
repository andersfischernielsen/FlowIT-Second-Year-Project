using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Exceptions;
using Event.Exceptions;
using Event.Interfaces;
using Event.Logic;

namespace Event.Controllers
{
    public class LifecycleController : ApiController
    {
        private readonly ILifecycleLogic _logic;

        // Constructor used by framework
        public LifecycleController()
        {
            _logic = new LifecycleLogic();
        }

        // Constructor used for dependency-injection
        public LifecycleController(ILifecycleLogic logic)
        {
            _logic = logic;
        }

        /// <summary>
        /// Sets up an Event at this WebAPI
        /// </summary>
        /// <param name="eventDto">The data (ruleset and initial state), this Event should be set to</param>
        /// <returns></returns>
        [Route("events")]
        [HttpPost]
        public async Task CreateEvent([FromBody] EventDto eventDto)
        {
            // Check that provided input can be mapped onto an instance of EventDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto an instance of EventDto."));
            }

            // Prepare for method-call: Gets own URI
            var s = string.Format("{0}://{1}", Request.RequestUri.Scheme, Request.RequestUri.Authority);
            var ownUri = new Uri(s);

            try
            {
                await _logic.CreateEvent(eventDto, ownUri);
            }
            catch (EventExistsException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "CreateEvent: Event already exists"));
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "CreateEvent: Seems input was not satisfactory"));
            }
            catch (FailedToPostEventAtServerException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "CreateEvent: Failed to Post Event at Server"));
            }
            catch (FailedToDeleteEventFromServerException)
            {
                // Is thrown if we somehow fail to PostEventToServer
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "CreateEvent: Failed to delete Event from Server. " +
                    "The deletion was attempted because, posting the Event to Server failed. "));
            }
            catch (FailedToCreateEventException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "CreateEvent: Failed to create Event locally "));
            }
            catch (Exception)
            {
                // Will catch any other Exception
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "Create Event: An un-expected exception arose"));
            }
        }

        /// <summary>
        /// DeleteEvent will delete an Event
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventId">The id of the Event to be deleted</param>
        /// <returns></returns>
        [Route("events/{workflowId}/{eventId}")]
        [HttpDelete]
        public async Task DeleteEvent(string workflowId, string eventId)
        {
            try
            {
                await _logic.DeleteEvent(workflowId, eventId);
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "DeleteEvent: Seems input was not satisfactory"));
            }
            catch (LockedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                    "DeleteEvent: Event is currently locked by someone else"));
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                   "DeleteEvent: Event does not exist"));
            }
            catch (FailedToDeleteEventFromServerException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "DeleteEvent: Failed to delete Event from Server"));
            }       
        }


        /// <summary>
        /// This method resets an Event. Note, that this will reset the three bool-values of the Event
        /// to their initial values, and reset any locks!. 
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventId">Id of the Event, that is to be reset</param>
        /// <param name="eventDto">Empty container</param>
        /// <returns></returns>
        [Route("events/{workflowId}/{eventId}/reset")]
        [HttpPut]
        public async Task ResetEvent(string workflowId, string eventId, [FromBody] EventDto eventDto)
        {
            if (workflowId == null || eventId == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "ResetEvent: Seems input was not satisfactory"));
            }
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "ResetEvent: Provided input could not be mapped onto an instance of EventDto."));
            }

            try
            {
                await _logic.ResetEvent(workflowId, eventId);
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "ResetEvent: Seems input was not satisfactory"));
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "ResetEvent: Event seems not to exist"));
            }
        }

        /// <summary>
        /// Get the entire Event, (namely rules and state for this Event)
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventId">The id of the Event, that you wish to get an EventDto representation of</param>
        /// <returns>A task containing a single EventDto which represents the Events current state.</returns>
        [Route("events/{workflowId}/{eventId}")]
        [HttpGet]
        public async Task<EventDto> GetEvent(string workflowId, string eventId)
        {
            try
            {
                return await _logic.GetEventDto(workflowId, eventId);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    workflowId + "." + eventId + " not found"));
            }
            catch (ArgumentNullException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
            }
        }
    }
}
