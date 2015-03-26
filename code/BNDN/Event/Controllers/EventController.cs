using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
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
        private IEventStorage Storage { get; set; }
        private EventControllerLogic LogicHandler { get; set; }

        public EventController()
        {
            // Fetches Singleton-storage
            Storage = InMemoryStorage.GetState();
            
            LogicHandler = new EventControllerLogic();
        }


        /// <summary>
        /// TODO: Where should this method properly be located?
        /// TODO: What is the intent of this method? What does it do?
        /// GetUriOfEvent 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/remoteuri")]
        public async Task<Uri> GetUriOfEvent()
        {
            Uri result = null;
            if (HttpContext.Current != null)
             {
                result = HttpContext.Current.Request.UserHostAddress == "::1"
                    ? new Uri("http://localhost:13752")
                    : new Uri(String.Format("http://{0}:13752/", HttpContext.Current.Request.UserHostAddress));
            }
            return await LogicHandler.IsEvent(result) ? result : null;
        }



        /// <summary>
        /// Get the entire Event, (namely including rules and state for this Event)
        /// </summary>
        /// <returns>A task resulting in a single EventDto which represents the Events current state.</returns>
        [Route("event")]
        [HttpGet]
        public async Task<EventDto> GetEvent()
        {
            return await Storage.EventDto;
        }


        /// <summary>
        /// PutEvent will update this Event using the content of the EventDto provided in the body of the PUT-request.
        /// </summary>
        /// <param name="eventDto">EventDto contains the information that that is to be used to update this event</param>
        /// <returns></returns>
        [Route("event")]
        [HttpPut]
        public async Task<IHttpActionResult> PutEvent([FromBody] EventDto eventDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Storage.EntireEventDto = eventDto;

            return await Task.Run(() => Ok());
        }
    }
}