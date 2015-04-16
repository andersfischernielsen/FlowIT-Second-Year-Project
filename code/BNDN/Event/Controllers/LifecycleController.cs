using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
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

            // Prepare for method-call: Gets own URI (i.e. http://address)
            var s = string.Format("{0}://{1}", Request.RequestUri.Scheme, Request.RequestUri.Authority);
            var ownUri = new Uri(s);

            // TODO: Now, call logic 
            try
            {
                await _logic.CreateEvent(eventDto, ownUri);
            }
            catch (ApplicationException)
            {                
                throw;
            }

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
            await _logic.DeleteEvent(eventId);
        }


        /// <summary>
        /// This method resets an Event. Note, that this will reset the three bool-values of the Event
        /// to their initial values, and reset any locks!. 
        /// </summary>
        /// <param name="eventId">Id of the Event, that is to be reset</param>
        /// <param name="eventDto">Empty container</param>
        /// <returns></returns>
        [Route("events/{eventId}/reset")]
        [HttpPut]
        public async Task ResetEvent([FromBody] EventDto eventDto, string eventId)
        {
            await _logic.ResetEvent(eventId);
        }

        /// <summary>
        /// Get the entire Event, (namely rules and state for this Event)
        /// </summary>
        /// <param name="eventId">The id of the Event, that you wish to get an EventDto representation of</param>
        /// <returns>A task containing a single EventDto which represents the Events current state.</returns>
        [Route("events/{eventId}")]
        [HttpGet]
        public async Task<EventDto> GetEvent(string eventId)
        {
            return await _logic.GetEventDto(eventId);
        }
    }
}
