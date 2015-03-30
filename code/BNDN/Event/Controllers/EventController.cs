using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Interfaces;
using Event.Models;

namespace Event.Controllers
{
    /// <summary>
    /// EventController handles requests regarding the Event as a whole; i.e. requests that manipulates all
    /// aspects of this Event. 
    /// </summary>
    public class EventController : ApiController
    {
        private IEventLogic Logic { get; set; }
        public EventController()
        {
            // Fetches Singleton Logic-layer
            Logic = EventLogic.GetState();
        }

        #region EventDto
        /// <summary>
        /// Get the entire Event, (namely rules and state for this Event)
        /// </summary>
        /// <returns>A task containing a single EventDto which represents the Events current state.</returns>
        [Route("event")]
        [HttpGet]
        public async Task<EventDto> GetEvent()
        {
            // Dismiss request if Event is currently locked
            if (Logic.IsLocked())
            {
                // Event is currently locked)
                StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            return await Logic.EventDto;
        }

        /// <summary>
        /// Sets up this Event, namely its rules and state
        /// </summary>
        /// <param name="eventDto">The data (ruleset and initial state), this Event should be set to</param>
        /// <returns></returns>
        [Route("event")]
        [HttpPost]
        public async Task PostEvent([FromBody] EventDto eventDto)
        {
            if (eventDto == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            // Dismiss request if Event is currently locked
            if (Logic.IsLocked())
            {
                // Event is currently locked)
                StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            // Prepare for method-call
            var ownUri = new Uri(Request.RequestUri.Authority);

            // Method call
            await Logic.InitializeEvent(eventDto, ownUri);


            Ok(true);
        }


        /// <summary>
        /// This method will override the current info held in this Event, with the data held in 
        /// the provided EventDto-argument passed to the method call
        /// </summary>
        /// <param name="eventDto">Holds the data that should override the current data held in this Event</param>
        /// <returns></returns>
        [Route("event")]
        [HttpPut]
        public async Task PutEvent([FromBody] EventDto eventDto)
        {
            // Dismiss request if Event is currently locked
            if (Logic.IsLocked())
            {
                // Event is currently locked)
                StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            // Prepare for method-call
            var ownUri = new Uri(Request.RequestUri.Authority);
            try
            {
                await Logic.UpdateEvent(eventDto, ownUri);
            }
            catch (NullReferenceException exception)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ModelState));
            }
            // TODO: Discuss - should we return Ok(true)?
            Ok(true);
        }


        [Route("event")]
        [HttpDelete]
        public async Task DeleteEvent()
        {
            // Dismiss request if Event is currently locked
            if (Logic.IsLocked())
            {
                // Event is currently locked)
                StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            try
            {
                await Logic.DeleteEvent();
            }
            catch (NullReferenceException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                "Event is not initialized!"));
            }
        }
        #endregion
    }
}