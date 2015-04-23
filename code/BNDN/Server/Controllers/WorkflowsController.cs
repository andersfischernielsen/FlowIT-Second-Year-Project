using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.History;
using Server.Exceptions;
using Server.Interfaces;
using Server.Logic;
using Server.Storage;

namespace Server.Controllers
{

    public class WorkflowsController : ApiController
    {
        private readonly IServerLogic _logic;
        private readonly IWorkflowHistoryLogic _historyLogic;

        public WorkflowsController()
        {
            _logic = new ServerLogic(new ServerStorage());
            _historyLogic = new WorkflowHistoryLogic();
        }

        public WorkflowsController(IServerLogic logic, IWorkflowHistoryLogic historyLogic)
        {
            _logic = logic;
            _historyLogic = historyLogic;
        }

        #region GET requests
        /// <summary>
        /// Returns a list of all workflows currently held at this Server
        /// </summary>
        /// <returns>List of WorkflowDto</returns>
        // GET: /workflows
        [Route("workflows")]
        public async Task<IEnumerable<WorkflowDto>> Get()
        {
            var toReturn = await _logic.GetAllWorkflows();
            await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
            {
                HttpRequestType = "GET",
                Message = "Called: Get",
                MethodCalledOnSender = "Get"
            });

            return toReturn;
        }


        // GET: /Workflows/5
        /// <summary>
        /// Given an workflowId, this method returns all events within that workflow
        /// </summary>
        /// <param name="workflowId">Id of the requested workflow</param>
        /// <returns>IEnumerable of EventAddressDto</returns>
        [Route("workflows/{workflowId}")]
        [HttpGet]
        public async Task<IEnumerable<EventAddressDto>> Get(string workflowId)
        {
            try
            {
                var toReturn = await _logic.GetEventsOnWorkflow(workflowId);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Called: Get",
                    HttpRequestType = "GET",
                    MethodCalledOnSender = "Get(" + workflowId + ")"
                });

                return toReturn;
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + toThrow.GetType(),
                    HttpRequestType = "GET",
                    MethodCalledOnSender = "Get(" + workflowId + ")"
                });

                throw toThrow;
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
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Provided input could not be mapped onto an instance of WorkflowDto."));
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostWorkflow",
                    WorkflowId = workflowDto.Id
                });

                throw toThrow;

            }

            if (workflowDto == null)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Data was not provided"));
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostWorkflow"
                });

                throw toThrow;
            }

            try
            {
                // Add this Event to the specified workflow
                await _logic.AddNewWorkflow(workflowDto);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Called: PostWorkflow",
                    MethodCalledOnSender = "PostWorkflow",
                    WorkflowId = workflowDto.Id
                });
            }
            catch (WorkflowAlreadyExistsException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                    "A workflow with that id exists!"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostWorkflow",
                    WorkflowId = workflowDto.Id
                });

                throw toThrow;
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostWorkflow",
                });

                throw toThrow;
            }
        }

        [Route("Workflows/{workflowId}")]
        [HttpPost]
        public async Task PostEventToWorkFlow(string workflowId, [FromBody] EventAddressDto eventToAddDto)
        {
            if (!ModelState.IsValid)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto EventAddressDto"));
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow",
                    WorkflowId = workflowId,
                    EventId = eventToAddDto.Id
                });

                throw toThrow;
            }

            try
            {
                // Add this Event to the specified workflow
                await _logic.AddEventToWorkflow(workflowId, eventToAddDto);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventToAddDto.Id,
                    Message = "Called: PostEventWorkflow",
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    HttpRequestType = "POST",
                    WorkflowId = workflowId
                });
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                });

                throw toThrow;
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
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                                "Provided input could not be mapped onto an EventAddressDto"));
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow",
                    WorkflowId = workflowId
                });
            }

            try
            {
                // Add this Event to the specified workflow
                await _logic.UpdateEventOnWorkflow(workflowId, eventToBeUpdated);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventToBeUpdated.Id,
                    WorkflowId = workflowId,
                    Message = "Called: UpdateEventOnWorkFlow",
                    HttpRequestType = "PUT",
                    MethodCalledOnSender = "UpdateEventOnWorkFlow(" + workflowId + ")"
                });
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow",
                    WorkflowId = workflowId
                });
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
                _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventId,
                    WorkflowId = workflowId,
                    Message = "Called: DeleteEventFromWorkflow",
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteEventFromWorkflow(" + workflowId + ", " + eventId + ")",
                });
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "Server: Failed to remove Event from workflow", ex));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventId,
                    WorkflowId = workflowId,
                    Message = "Threw: " + toThrow.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteEventFromWorkflow(" + workflowId + ", " + eventId + ")",
                });

                throw toThrow;
            }
        }

        [Route("Workflows/{workflowId}")]
        [HttpDelete]
        public async Task DeleteWorkflow(string workflowId)
        {
            try
            {
                await _logic.RemoveWorkflow(workflowId);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Called: DeleteWorkflow",
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")",
                });
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "Server: Failed to remove workflow", ex));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + toThrow.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")"
                });

                throw toThrow;
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            base.Dispose(disposing);
        }
    }
}
