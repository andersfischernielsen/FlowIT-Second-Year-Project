using System;
using System.Threading.Tasks;
using System.Web.Http;
using Common.DTO.Event;
using Common.DTO.History;
using Common.DTO.Shared;
using Common.Exceptions;
using Server.Exceptions;
using Server.Interfaces;

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
        public async Task<IHttpActionResult> Get()
        {
            var toReturn = await _logic.GetAllWorkflows();
            await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
            {
                HttpRequestType = "GET",
                Message = "Succesfully called: Get",
                MethodCalledOnSender = "Get"
            });

            return Ok(toReturn);
        }

        /// <summary>
        /// Given an workflowId, this method returns all events within that workflow
        /// </summary>
        /// <param name="workflowId">Id of the requested workflow</param>
        /// <returns>IEnumerable of EventAddressDto</returns>
        [Route("workflows/{workflowId}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(string workflowId)
        {
            try
            {
                var toReturn = await _logic.GetEventsOnWorkflow(workflowId);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Succesfully called: Get",
                    HttpRequestType = "GET",
                    MethodCalledOnSender = "Get(" + workflowId + ")"
                });

                return Ok(toReturn);
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "GET",
                    MethodCalledOnSender = "GET(" + workflowId + ")"
                }).Wait();
                return BadRequest("Seems input was not satisfactory");
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "GET",
                    MethodCalledOnSender = "Get(" + workflowId + ")"
                }).Wait();
                return NotFound();
            }
            catch (Exception e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "GET",
                    MethodCalledOnSender = "Get(" + workflowId + ")"
                }).Wait();

                return InternalServerError(e);
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
        public async Task<IHttpActionResult> PostWorkFlow([FromBody] WorkflowDto workflowDto)
        {
            if (!ModelState.IsValid)
            {
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = ModelState.ToString(),
                    MethodCalledOnSender = "PostWorkflow",
                    WorkflowId = workflowDto.Id
                });

                return BadRequest(ModelState);
            }

            try
            {
                // Add this Event to the specified workflow
                await _logic.AddNewWorkflow(workflowDto);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Succesfully called: PostWorkflow",
                    MethodCalledOnSender = "PostWorkflow",
                    WorkflowId = workflowDto.Id
                });
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "PostWorkflow",
                    WorkflowId = workflowDto != null ? workflowDto.Id : ""
                }).Wait();
                return BadRequest("Seems input was not satisfactory");
            }
            catch (WorkflowAlreadyExistsException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "PostWorkflow",
                    WorkflowId = workflowDto.Id
                }).Wait();
                return Conflict();
            }
            catch (Exception e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "PostWorkflow",
                }).Wait();

                return InternalServerError(e);
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
        public async Task<IHttpActionResult> PostEventToWorkFlow(string workflowId, [FromBody] EventAddressDto eventToAddDto)
        {
            if (!ModelState.IsValid)
            {
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = ModelState.ToString(),
                    MethodCalledOnSender = "PostEventWorkflow",
                    WorkflowId = workflowId,
                    EventId = eventToAddDto.Id
                });
                return BadRequest(ModelState);
            }

            try
            {
                // Add this Event to the specified workflow
                await _logic.AddEventToWorkflow(workflowId, eventToAddDto);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventToAddDto.Id,
                    Message = "Succesfully called: PostEventWorkflow",
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    HttpRequestType = "POST",
                    WorkflowId = workflowId
                });
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                }).Wait();
                return BadRequest("Seems input was not satisfactory");
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                }).Wait();

                return NotFound();
            }
            catch (EventExistsException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                }).Wait();

                return BadRequest("The event already exists at Server. You may wish to update the Event instead, using a PUT call");
            }
            catch (Exception e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "PostEventWorkflow(" + workflowId + ")",
                    WorkflowId = workflowId
                }).Wait();

                return InternalServerError(e);
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
        public async Task<IHttpActionResult> DeleteEventFromWorkflow(string workflowId, string eventId)
        {
            try
            {
                // Delete the given event id from the list of workflow-events.
                await _logic.RemoveEventFromWorkflow(workflowId, eventId);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventId,
                    WorkflowId = workflowId,
                    Message = "Succesfully called: DeleteEventFromWorkflow",
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteEventFromWorkflow(" + workflowId + ", " + eventId + ")",
                });
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventId,
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteEventFromWorkflow(" + workflowId + ", " + eventId + ")",
                }).Wait();

                return BadRequest("Seems input was not satisfactory");
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventId,
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteEventFromWorkflow(" + workflowId + ", " + eventId + ")",
                }).Wait();

                return NotFound();
            }
            catch (Exception e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = eventId,
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteEventFromWorkflow(" + workflowId + ", " + eventId + ")",
                }).Wait();

                return InternalServerError(e);
            }
        }

        /// <summary>
        /// Deletes the specified workflow
        /// </summary>
        /// <param name="workflowId">Id of the workflow, that is to be deleted</param>
        /// <returns></returns>
        [Route("Workflows/{workflowId}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteWorkflow(string workflowId)
        {
            try
            {
                await _logic.RemoveWorkflow(workflowId);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Succesfully called: DeleteWorkflow",
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")",
                });
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")"
                }).Wait();

                return BadRequest("Seems input was not satisfactory");
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")"
                }).Wait();

                return NotFound();
            }
            catch (Exception e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    WorkflowId = workflowId,
                    Message = "Threw: " + e.GetType(),
                    HttpRequestType = "DELETE",
                    MethodCalledOnSender = "DeleteWorkflow(" + workflowId + ")"
                }).Wait();

                return InternalServerError(e);
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            _historyLogic.Dispose();
            base.Dispose(disposing);
        }
    }
}
