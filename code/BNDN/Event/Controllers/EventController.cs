using System;
using System.Collections.Generic;
using System.Linq;
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

        [Route("")]
        [HttpPost]
        public async Task PostEvent([FromBody] EventDto eventDto)
        {
            // Todo: Fix
            var logic = (EventLogic)Logic;
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            if (eventDto == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data was null"));
            }
            if (logic.EventId != null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Event is already running!"));
            }

            logic.EventId = eventDto.EventId;
            logic.WorkflowId = eventDto.WorkflowId;
            logic.Name = eventDto.Name;
            logic.Included = eventDto.Included;
            logic.Pending = eventDto.Pending;
            logic.Executed = eventDto.Executed;
            logic.Inclusions = Task.Run(() => new HashSet<Uri>(eventDto.Inclusions));
            logic.Exclusions = Task.Run(() => new HashSet<Uri>(eventDto.Exclusions));
            logic.Conditions = Task.Run(() => new HashSet<Uri>(eventDto.Conditions));
            logic.Responses = Task.Run(() => new HashSet<Uri>(eventDto.Responses)); 
            logic.OwnUri = new Uri(Request.RequestUri.Authority);

            var dto = new EventAddressDto
            {
                Id = logic.EventId,
                Uri = logic.OwnUri
            };

            // Todo: Server address.
            IServerFromEvent commuicator = new ServerCommunicator("http://serveraddress.azurewebsites.net", logic.EventId, logic.WorkflowId);

            var otherEvents = await commuicator.PostEventToServer(dto);

            foreach (var otherEvent in otherEvents)
            {
                await Logic.RegisterIdWithUri(otherEvent.Id, otherEvent.Uri);
            }
        }

        [Route("")]
        [HttpPut]
        public async Task PutEvent([FromBody] EventDto eventDto)
        {
            var logic = (EventLogic)Logic;
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            if (eventDto == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Data was null"));
            }
            if (logic.EventId == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Event is not initialized!"));
            }
            if (logic.EventId != eventDto.EventId || logic.WorkflowId != eventDto.WorkflowId)
            {
                //Todo remove from server and add again.
            }

            logic.EventId = eventDto.EventId;
            logic.WorkflowId = eventDto.WorkflowId;
            logic.Name = eventDto.Name;
            logic.Included = eventDto.Included;
            logic.Pending = eventDto.Pending;
            logic.Executed = eventDto.Executed;
            logic.Inclusions = Task.Run(() => new HashSet<Uri>(eventDto.Inclusions));
            logic.Exclusions = Task.Run(() => new HashSet<Uri>(eventDto.Exclusions));
            logic.Conditions = Task.Run(() => new HashSet<Uri>(eventDto.Conditions));
            logic.Responses = Task.Run(() => new HashSet<Uri>(eventDto.Responses));

            // Todo: This should not be necessary..
            logic.OwnUri = new Uri(Request.RequestUri.Authority);

            var dto = new EventAddressDto
            {
                Id = logic.EventId,
                Uri = logic.OwnUri
            };

            // Todo: Server address.
            IServerFromEvent commuicator = new ServerCommunicator("http://serveraddress.azurewebsites.net", logic.EventId, logic.WorkflowId);

            var otherEvents = await commuicator.PostEventToServer(dto);


            // Todo clear old registered events!
            foreach (var otherEvent in otherEvents)
            {
                await Logic.RegisterIdWithUri(otherEvent.Id, otherEvent.Uri);
            }
        }

        [Route("event")]
        [HttpDelete]
        public async Task DeleteEvent()
        {
            var logic = (EventLogic) Logic;
            if (logic.EventId == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Event is not initialized!"));
            }

            // Todo: Server address.
            IServerFromEvent commuicator = new ServerCommunicator("http://serveraddress.azurewebsites.net", logic.EventId, logic.WorkflowId);

            await commuicator.DeleteEventFromServer();

            await Logic.ResetState();
        }
        #endregion
    }
}