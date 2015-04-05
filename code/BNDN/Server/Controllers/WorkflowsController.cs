using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Common;
using Server.Storage;

namespace Server.Controllers
{

    public class WorkflowsController : ApiController
    {
        private IServerLogic ServerLogic { get; set; }

        public WorkflowsController()
        {
            ServerLogic = new ServerLogic(new ServerStorage());
            //ServerLogic = new ServerLogic(CacheStorage.GetStorage);
            //ServerLogic = new ServerLogic(new WorkflowStorage());
        }

        public WorkflowsController(IServerLogic serverLogic)
        {
            ServerLogic = serverLogic;
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


        #region GET requests
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
                return ServerLogic.GetEventsOnWorkflow(workflowId);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }

        }

        // GET: /Login
        /// <summary>
        /// Returns the users roles on all workflows.
        /// </summary>
        /// <param name="username">Id of the requested workflow</param>
        /// <returns>RolesOnWorkflowsDto</returns>
        [Route("login/{username}")]
        [HttpGet]
        public RolesOnWorkflowsDto Login(string username)
        {
            try
            {
                var result = ServerLogic.Login(username);
                return result;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }

        }
        #endregion

        #region POST requests
        /// <summary>
        /// PostNewWorkFlow adds a new workflow.
        /// </summary>
        /// <param name="workflowDto"></param>
        [Route("Workflows")]
        [HttpPost]
        public void PostWorkFlow([FromBody] WorkflowDto workflowDto)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of WorkflowDto"));
            }

            // TODO: see that workflowId matches the dto.
            try
            {
                // Add this Event to the specified workflow
                ServerLogic.AddNewWorkflow(workflowDto);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        [Route("Workflows/{workflowId}")]
        [HttpPost]
        public IEnumerable<EventAddressDto> PostEventToWorkFlow(string workflowId, [FromBody] EventAddressDto eventToAddDto)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto workflowDto"));
            }

            try
            {
                // Add this Event to the specified workflow
                ServerLogic.AddEventToWorkflow(workflowId, eventToAddDto);

                // To caller, return a list of the other (excluding itself) Events on the workflow
                return ServerLogic.GetEventsOnWorkflow(workflowId).Where(eventAddressDto => eventAddressDto.Id != eventToAddDto.Id);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }
        #endregion


        #region PUT requests
        /// <summary>
        /// UpdateEventToWorkFlow updates an Event in a workflow with the specified workflowid. 
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="eventToBeUpdated"></param>
        [Route("Workflows/{workflowId}")]
        [HttpPut]
        public void UpdateEventToWorkFlow(string workflowId, [FromBody] EventAddressDto eventToBeUpdated)
        {
            // Check if provided input can be mapped onto an EventAddressDto
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto an EventAddressDto"));
            }

            try
            {
                // Add this Event to the specified workflow
                ServerLogic.UpdateEventOnWorkflow(workflowId, eventToBeUpdated);
            }
            catch (Exception ex)
            {

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }
        #endregion

        #region DELETE requests
        [Route("Workflows/{workflowId}/{eventId}")]
        [HttpDelete]
        public void DeleteEventFromWorkflow(string workflowId, string eventId)
        {
            try
            {
                // Delete the given event id from the list of workflow-events.
                ServerLogic.RemoveEventFromWorkflow(workflowId, eventId);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Server: Failed to remove Event from workflow", ex));
            }
        }

        [Route("Workflows/{workflowId}")]
        [HttpDelete]
        public void DeleteWorkflow(string workflowId)
        {
            try
            {
                ServerLogic.RemoveWorkflow(ServerLogic.GetWorkflow(workflowId));
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Server: Failed to remove workflow", ex));
            }
        }
        #endregion
    }
}
