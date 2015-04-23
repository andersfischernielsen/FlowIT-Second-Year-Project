using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Exceptions;
using Common.History;
using Event.Interfaces;
using Event.Logic;

namespace Event.Controllers
{
    public class LifecycleController : ApiController
    {
        private readonly ILifecycleLogic _logic;
        private readonly IEventHistoryLogic _historyLogic;

        // Constructor used by framework
        public LifecycleController()
        {
            _logic = new LifecycleLogic();
            _historyLogic = new EventHistoryLogic();
        }

        // Constructor used for dependency-injection
        public LifecycleController(ILifecycleLogic logic, IEventHistoryLogic historyLogic)
        {
            _logic = logic;
            _historyLogic = historyLogic;
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
                // For nicer debugging, a list of the conflicting mappings is provided
                var conflictElements = new StringBuilder();
                foreach (var key in ModelState.Keys)
                {
                    if (!ModelState.IsValidField(key))
                    {
                        conflictElements.Append(key + ", ");
                    }
                }

                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto an instance of EventDto " +
                                                conflictElements));

                await _historyLogic.SaveException(toThrow, eventDto.EventId, eventDto.WorkflowId);
                throw toThrow;
            }

            if (eventDto == null)
            {
                // TODO: Provide nicer description / error message
                var toThrow = new HttpResponseException(HttpStatusCode.BadRequest);
                await _historyLogic.SaveException(toThrow, "POST", "CreateEvent");
            }

            // Prepare for method-call: Gets own URI (i.e. http://address)
            var uri = string.Format("{0}://{1}", Request.RequestUri.Scheme, Request.RequestUri.Authority);
            var ownUri = new Uri(uri);

            // TODO: Exception handling
            await _logic.CreateEvent(eventDto, ownUri);
            await _historyLogic.SaveSuccesfullCall("POST", "CreateEvent", eventDto.EventId, eventDto.WorkflowId);
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
                await _historyLogic.SaveSuccesfullCall("DELETE", "DeleteEvent", eventId, workflowId);
            }
            catch (LockedException ex)
                // Todo: Exception handling
            {
                _historyLogic.SaveException(ex, "DELETE", "DeleteEvent", eventId, workflowId);
                throw;
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
            try
            {
                await _logic.ResetEvent(workflowId, eventId);
                await _historyLogic.SaveSuccesfullCall("PUT", "ResetEvent", eventId, workflowId);

            }
                // Todo: Exception handling
            catch (Exception ex) {
                _historyLogic.SaveException(ex, "PUT", "ResetEvent", eventId, workflowId);
                throw;
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
                 var toReturn = await _logic.GetEventDto(workflowId, eventId);
                 await _historyLogic.SaveSuccesfullCall("GET", "GetEvent", eventId, workflowId);

                 return toReturn;
            }
            catch (NotFoundException ex) {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, workflowId + "." + eventId + " not found"));
                _historyLogic.SaveException(toThrow, "GET", "GetEvent", eventId, workflowId);

                throw toThrow;
            }
                // Todo: Exception handling.
            catch (Exception ex) {
                _historyLogic.SaveException(ex, "GET", "GetEvent", eventId, workflowId);
                throw;
            }
        }
    }
}
