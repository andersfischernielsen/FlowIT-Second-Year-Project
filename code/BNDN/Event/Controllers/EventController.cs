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
    /// The EventController is responsible for handling the Api requests on the {host}/event/* service.
    /// It must both handle incoming requests from other Events and incoming requests from Clients.
    /// </summary>
    public class EventController : ApiController
    {
        private IEventLogic Logic { get; set; }
        public EventController()
        {
            Logic = EventLogic.GetState();
        }

        #region EventDto
        /// <summary>
        /// Get the entire Event, (namely including rules and state for this Event)
        /// </summary>
        /// <returns>A task resulting in a single EventDto which represents the Events current state.</returns>
        [Route("event")]
        [HttpGet]
        public async Task<EventDto> GetEvent()
        {
            return await Logic.EventDto;
        }

        [Route("event")]
        [HttpPost]
        public async Task PostEvent([FromBody] EventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            // Prepare for method-call
            var ownUri = new Uri(Request.RequestUri.Authority);
            
            // Method call
            try
            {
                await Logic.InitializeEvent(eventDto, ownUri);
            }
            catch (NullReferenceException exception)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ModelState));
            }
        }

        [Route("event")]
        [HttpPut]
        public async Task PutEvent([FromBody] EventDto eventDto)
        {
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
        }


        [Route("event")]
        [HttpDelete]
        public async Task DeleteEvent()
        {
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