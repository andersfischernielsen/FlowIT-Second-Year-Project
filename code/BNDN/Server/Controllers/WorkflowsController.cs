using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Exceptions;
using Common.History;
using Server.Exceptions;
using Server.Interfaces;
using Server.Logic;
using Server.Storage;

namespace Server.Controllers
{
    /// <summary>
    /// WorkflowsController handles HTTP-request regarding workflows on Server
    /// </summary>
    public class WorkflowsController : ApiController
    {
        private readonly IServerLogic _logic;
        private readonly IWorkflowHistoryLogic _historyLogic;

        /// <summary>
        /// Default constructor used during runtime
        /// </summary>
        public WorkflowsController()
        {
            _logic = new ServerLogic(new ServerStorage());
            _historyLogic = new WorkflowHistoryLogic();
        }

        /// <summary>
        /// Constructor used for dependency-injection udring testing
        /// </summary>
        /// <param name="logic">Logic that handles logic for workflows operations</param>
        /// <param name="historyLogic">Logic that handles logic for history operations, i.e. recording successfull
        /// or non-successfull operations happening at Server</param>
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
        [Route("workflows")]
        [HttpGet]
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
            catch (ArgumentNullException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + toThrow.GetType(),
                    HttpRequestType = "GET",
                    MethodCalledOnSender = "GET(" + workflowId + ")"
                }).Wait();
                throw toThrow;
            }
            catch (NotFoundException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "The specified workflow could not be found"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + toThrow.GetType(),
                    HttpRequestType = "GET",
                    MethodCalledOnSender = "Get(" + workflowId + ")"
                }).Wait();
                throw toThrow;
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
                }).Wait();

                throw toThrow;
            }
        }
        #endregion

        #region POST requests
        /// <summary>
        /// PostNewWorkFlow adds a new workflow.
        /// </summary>
        /// <param name="workflowDto">Contains the information on the workflow, that is to be created at Server</param>
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

            if (workflowDto == null)    // TODO: Should be obsolete now; because we already check ModelState.IsValid, workflowDto should not be able to be null
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
            catch (ArgumentNullException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostWorkflow",
                    WorkflowId = workflowDto.Id
                }).Wait();
                throw toThrow;
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
                }).Wait();

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
                }).Wait();
                throw toThrow;
            }
        }

        /// <summary>
        /// Will add an Event to a workflow. 
        /// </summary>
        /// <param name="workflowId">The id of the workflow, that the Event is to be added to</param>
        /// <param name="eventToAddDto">Contains information about the Event</param>
        /// <returns></returns>
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
            catch (ArgumentNullException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                }).Wait();
                throw toThrow;
            }
            catch (NotFoundException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "The workflow was not found at Server"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                }).Wait();
                throw toThrow;
            }
            catch (EventExistsException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "The event already exists at Server. You may wish to update the Event instead, using a PUT call"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                }).Wait();
                throw toThrow;
            }
            catch (IllegalStorageStateException ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,ex));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                }).Wait();
                throw toThrow;
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
                }).Wait();
                throw toThrow;
            }
        }
        #endregion

        #region PUT requests
        // TODO: Discuss: Is this method ever used? Should we not delete...? Or do we keep it to stay REST'ed...?
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
            catch (ArgumentNullException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow",
                    WorkflowId = workflowId
                }).Wait();
                throw toThrow;
            }
            catch (NotFoundException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "Either the Event or the workflow could not be found at Server"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow",
                    WorkflowId = workflowId
                }).Wait();
                throw toThrow;
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
                }).Wait();
                throw toThrow;
            }
        }
        #endregion

        #region DELETE requests
        /// <summary>
        /// Will delete a specified Event from a specified workflow
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the Event, that is to be deleted</param>
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
            catch (ArgumentNullException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
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
            catch (NotFoundException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "Either the event or the workflow was not found at Server"));
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

        /// <summary>
        /// Deletes the specified workflow
        /// </summary>
        /// <param name="workflowId">Id of the workflow, that is to be deleted</param>
        /// <returns></returns>
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
            catch (ArgumentNullException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + toThrow.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")"
                }).Wait();
                throw toThrow;
            }
            catch (NotFoundException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "The workflow could not be found"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + toThrow.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")"
                }).Wait();

                throw toThrow;
            }
            catch (IllegalStorageStateException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    "Server: Storage was found in an illegal state"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + toThrow.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")"
                }).Wait();

                throw toThrow;
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
                }).Wait();

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
