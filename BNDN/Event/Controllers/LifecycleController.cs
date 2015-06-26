using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common.DTO.Event;
using Common.Exceptions;
using Event.Interfaces;
using Event.Logic;

namespace Event.Controllers
{
    /// <summary>
    /// LifecycleController handles handles HTTP-request regarding Event lifecycle.
    /// </summary>
    public class LifecycleController : ApiController
    {
        private readonly ILifecycleLogic _logic;
        private readonly IEventHistoryLogic _historyLogic;

        /// <summary>
        /// Constructor used for dependency-injection
        /// </summary>
        /// <param name="logic">Logic-layer implementing the ILifecycleLogic interface</param>
        /// <param name="historyLogic">Historylogic-layer implementing the IEventHistoryLogic interface</param>
        public LifecycleController(ILifecycleLogic logic, IEventHistoryLogic historyLogic)
        {
            if (logic == null || historyLogic == null)
            {
                throw new ArgumentNullException();
            }
            _logic = logic;
            _historyLogic = historyLogic;
        }

        /// <summary>
        /// Sets up an Event at this WebAPI. It will attempt to post the (needed details of the) Event to Server.  
        /// </summary>
        /// <param name="eventDto">Containts the data, this Event should be initially set to</param>
        /// <returns></returns>
        [Route("events")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateEvent([FromBody] EventDto eventDto)
        {

            // Check that provided input can be mapped onto an instance of EventDto
            if (!ModelState.IsValid)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of EventDto."));
                await _historyLogic.SaveException(toThrow, eventDto.EventId, eventDto.WorkflowId);
                return BadRequest(ModelState);
            }


            // Prepare for method-call: Gets own URI
            var s = string.Format("{0}://{1}", Request.RequestUri.Scheme, Request.RequestUri.Authority);
            var ownUri = new Uri(s);

            try
            {
                await _logic.CreateEvent(eventDto, ownUri);
                await _historyLogic.SaveSuccesfullCall("POST", "CreateEvent", eventDto.EventId, eventDto.WorkflowId);
                return Ok();
            }
            catch (EventExistsException e)
            {
                _historyLogic.SaveException(e, "POST", "CreateEvent");
                return Conflict();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "POST", "CreateEvent");
                return BadRequest("CreateEvent: Seems input was not satisfactory");
            }
            catch (Exception e)
            {
                // Will catch any other Exception
                _historyLogic.SaveException(e, "POST", "CreateEvent");

                return InternalServerError(e);
            }
        }

        /// <summary>
        /// DeleteEvent will delete an Event at this Event-machine, and attempt aswell to delete the Event from Server.
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventId">The id of the Event to be deleted</param>
        /// <returns></returns>
        [Route("events/{workflowId}/{eventId}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteEvent(string workflowId, string eventId)
        {
            try
            {
                await _logic.DeleteEvent(workflowId, eventId);
                await _historyLogic.SaveSuccesfullCall("DELETE", "DeleteEvent", eventId, workflowId);
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "DELETE", "DeleteEvent");
                return BadRequest("DeleteEvent: Seems input was not satisfactory");
            }
            catch (LockedException e)
            {
                _historyLogic.SaveException(e, "DELETE", "DeleteEvent");
                return Conflict();
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "DELETE", "DeleteEvent");
                return NotFound();
            }
            catch (Exception e)
            {
                _historyLogic.SaveException(e, "DELETE", "DeleteEvent");
                return InternalServerError(e);
            }
        }


        /// <summary>
        /// This method resets an Event. Note, that this will reset the three bool-values of the Event
        /// to their initial values, and reset any locks!. 
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventId">EventId of the Event, that is to be reset</param>
        /// <returns></returns>
        [Route("events/{workflowId}/{eventId}/reset")]
        [HttpPut]
        public async Task<IHttpActionResult> ResetEvent(string workflowId, string eventId)
        {
            try
            {
                await _logic.ResetEvent(workflowId, eventId);
                await _historyLogic.SaveSuccesfullCall("PUT", "ResetEvent", eventId, workflowId);
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "PUT", "ResetEvent", eventId, workflowId);
                return BadRequest("ResetEvent: Seems input was not satisfactory");
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "PUT", "ResetEvent", eventId, workflowId);
                return NotFound();
            }
            catch (Exception e)
            {
                _historyLogic.SaveException(e, "PUT", "ResetEvent", eventId, workflowId);
                return InternalServerError(e);
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
        public async Task<IHttpActionResult> GetEvent(string workflowId, string eventId)
        {
            try
            {
                var toReturn = await _logic.GetEventDto(workflowId, eventId);
                await _historyLogic.SaveSuccesfullCall("GET", "GetEvent", eventId, workflowId);

                return Ok(toReturn);
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveException(e, "GET", "GetEvent", eventId, workflowId);
                return NotFound();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveException(e, "GET", "GetEvent", eventId, workflowId);
                return BadRequest("Seems input was not satisfactory");
            }
            catch (Exception e)
            {
                _historyLogic.SaveException(e, "GET", "GetEvent", eventId, workflowId);
                return InternalServerError(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _historyLogic.Dispose();
            _logic.Dispose();
            base.Dispose(disposing);
        }
    }
}
