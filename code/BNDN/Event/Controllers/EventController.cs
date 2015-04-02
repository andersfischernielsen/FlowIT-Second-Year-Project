using System;
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

        #region EventDto
        /// <summary>
        /// Get the entire Event, (namely rules and state for this Event)
        /// </summary>
        /// <returns>A task containing a single EventDto which represents the Events current state.</returns>
        [Route("event")]
        [HttpGet]
        public async Task<EventDto> GetEvent()
        {
            using (IEventLogic logic = new EventLogic())
            {
                // Dismiss request if Event is currently locked

                if (logic.IsLocked())
                {
                    // Event is currently locked)
                    StatusCode(HttpStatusCode.MethodNotAllowed);
                }

                return await logic.EventDto;
            }
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
            using (IEventLogic logic = new EventLogic())
            {
                // Dismiss request if Event is currently locked
                if (logic.IsLocked())
                {
                    // Event is currently locked)
                    StatusCode(HttpStatusCode.MethodNotAllowed);
                }

                if (!ModelState.IsValid)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
                }

                // Prepare for method-call: Gets own URI (i.e. http://address)
                var s = string.Format("{0}://{1}", Request.RequestUri.Scheme, Request.RequestUri.Authority);
                var ownUri = new Uri(s);

                // Method call
                await logic.InitializeEvent(eventDto, ownUri);
            }
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
            using (IEventLogic logic = new EventLogic())
            {
                // Dismiss request if Event is currently locked
                if (logic.IsLocked())
                {
                    // Event is currently locked)
                    StatusCode(HttpStatusCode.MethodNotAllowed);
                }

                if (!ModelState.IsValid)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
                }

                // Prepare for method-call
                var ownUri = new Uri(string.Format("{0}://{1}", Request.RequestUri.Scheme, Request.RequestUri.Authority));

                try
                {
                    await logic.UpdateEvent(eventDto, ownUri);
                }
                catch (NullReferenceException)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        ModelState));
                }
            }
        }


        [Route("event")]
        [HttpDelete]
        public async Task DeleteEvent()
        {
            using (IEventLogic logic = new EventLogic())
            {
                // Dismiss request if Event is currently locked
                if (logic.IsLocked())
                {
                    // Event is currently locked)
                    StatusCode(HttpStatusCode.MethodNotAllowed);
                }

                try
                {
                    await logic.DeleteEvent();
                }
                catch (NullReferenceException)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Event is not initialized!"));
                }
            }
        }

        #endregion
    }
}