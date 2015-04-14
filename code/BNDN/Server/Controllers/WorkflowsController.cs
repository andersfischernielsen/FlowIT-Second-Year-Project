using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using Common;
using Server.Storage;

namespace Server.Controllers
{

    public class WorkflowsController : ApiController
    {
        private readonly IServerLogic _logic;

        public WorkflowsController() : this(new ServerLogic(new ServerStorage())) { }

        public WorkflowsController(IServerLogic logic)
        {
            _logic = logic;
        }

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Returns a list of all workflows currently held at this Server
        /// </summary>
        /// <returns>List of WorkflowDto</returns>
        // GET: /workflows
        [Route("workflows")]
        public IEnumerable<WorkflowDto> Get()
        {
            return _logic.GetAllWorkflows();
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
                return _logic.GetEventsOnWorkflow(workflowId);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
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
        public async Task PostWorkFlow([FromBody] WorkflowDto workflowDto)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of WorkflowDto"));
            }
            try
            {
                // Add this Event to the specified workflow
                await _logic.AddNewWorkflow(workflowDto);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        [Route("Workflows/{workflowId}")]
        [HttpPost]
        public async Task<IEnumerable<EventAddressDto>> PostEventToWorkFlow(string workflowId, [FromBody] EventAddressDto eventToAddDto)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto EventAddressDto"));
            }
            try
            {
                // Add this Event to the specified workflow
                await _logic.AddEventToWorkflow(workflowId, eventToAddDto);

                // To caller, return a list of the other (excluding itself) Events on the workflow
                return
                    _logic.GetEventsOnWorkflow(workflowId)
                        .Where(eventAddressDto => eventAddressDto.Id != eventToAddDto.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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
        public async Task UpdateEventToWorkFlow(string workflowId, [FromBody] EventAddressDto eventToBeUpdated)
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
                await _logic.UpdateEventOnWorkflow(workflowId, eventToBeUpdated);
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
                _logic.RemoveEventFromWorkflow(workflowId, eventId);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Server: Failed to remove Event from workflow", ex));
            }
        }

        [Route("Workflows/{workflowId}")]
        [HttpDelete]
        public async Task DeleteWorkflow(string workflowId)
        {
            try
            {
                await _logic.RemoveWorkflow(_logic.GetWorkflow(workflowId));
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
