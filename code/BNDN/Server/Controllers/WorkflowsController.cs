using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Common;
using Server.Models;
using Server.Storage;

namespace Server.Controllers
{
    
    public class WorkflowsController : ApiController
    {
        private IServerLogic ServerLogic { get; set; }

        public WorkflowsController()
        {
            ServerLogic = new ServerLogic(CacheStorage.GetStorage);
        }


        /// <summary>
        /// Returns a list of all workflows currently held at this Server
        /// </summary>
        /// <returns>List of WorkflowDto</returns>
        // GET: /workflows
        [Route("workflows")]
        public IEnumerable<WorkflowDto> Get()
        {
            return ServerLogic.GetAllWorkflows();
        }


        // GET: /Workflows/5
        /// <summary>
        /// Given an workflowId, this method returns all events within that workflow
        /// </summary>
        /// <param name="workflowId">Id of the requested workflow</param>
        /// <returns>IEnumerable of EventAddressDto</returns>
        [Route("workflows/{workflowId}")]
        [HttpGet]
        public IEnumerable<EventAddressDto> Get(string workflowId)
        {
            try
            {
                Debug.WriteLine("Hmm, we got here!");
                return ServerLogic.GetEventsOnWorkflow(workflowId);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
            
        }


        /// <summary>
        /// PostNewWorkFlow adds a new workflow with the specified workflowid. 
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventToAddDto"></param>
        [Route("Workflows/{workflowId}")]
        [HttpPost]
        // TODO: Clarify what information should Event provide to Server, when submitting itself to Server?
        // TODO: How does an Event know that an eventId is not already taken?
        public void PostWorkFlow(string workflowId, [FromBody] WorkflowDto dto)
        {
            // Todo see that workflowId matches the dto.
            try
            {
                // Add this Event to the specified workflow
                ServerLogic.AddNewWorkflow(dto);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,ex));
            }
        }

        /// <summary>
        /// PostEventToWorkFlow adds an Event to a workflow with the specified workflowid. 
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventToAddDto"></param>
        [Route("Workflows/{workflowId}")]
        [HttpPut]
        // TODO: Clarify what information should Event provide to Server, when submitting itself to Server?
        // TODO: How does an Event know that an eventId is not already taken?
        public void UpdateEventToWorkFlow(string workflowId, [FromBody] EventAddressDto eventToBeUpdated)
        {
            try
            {
            // Add this Event to the specified workflow
                ServerLogic.UpdateEventOnWorkflow(workflowId, eventToBeUpdated);
            }
            catch (Exception ex)
            {
                
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,ex.Message));
            }
        }

        [Route("Workflows/{workflowId}/{eventId}")]
        [HttpPost]
        // TODO: Clarify what information should Event provide to Server, when submitting itself to Server?
        // TODO: How does an Event know that an eventId is not already taken?
        public IEnumerable<EventAddressDto> PostEventToWorkFlow(string workflowId, string eventId, [FromBody] EventAddressDto eventToAddDto)
        {
            try
            {
                // Add this Event to the specified workflow
                ServerLogic.AddEventToWorkflow(workflowId, eventToAddDto);
                return ServerLogic.GetEventsOnWorkflow(workflowId).Where(eventAddressDto => eventAddressDto.Id != eventId);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        [Route("Workflows/{workflowId}/{eventId}")]
        [HttpDelete]
        // TODO: Is there any need to supply more than workflowId and eventId of the event that is to be removed?
        public void DeleteEventFromWorkflow(string workflowId, string eventId)
        {
            try
            {
                // Delete the given event id from the list of workflow-events.
                Debug.WriteLine("Yep, we got here!");
                ServerLogic.RemoveEventFromWorkflow(workflowId,eventId);
            }
            catch (Exception ex)
            {
                
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,ex));
            }
        }
    }
}
