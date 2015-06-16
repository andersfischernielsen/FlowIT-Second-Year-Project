using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common.DTO.History;
using Common.Exceptions;
using Server.Interfaces;
using Server.Logic;

namespace Server.Controllers 
{
    /// <summary>
    /// HistoryController handles HTTP-request regarding History on Server
    /// </summary>
    public class HistoryController : ApiController 
    {
        private readonly IWorkflowHistoryLogic _historyLogic;

        /// <summary>
        /// Used for dependency-injection
        /// </summary>
        /// <param name="historyLogic">Implementation of IWorkflowHistoryLogic</param>
        public HistoryController(IWorkflowHistoryLogic historyLogic)
        {
            _historyLogic = historyLogic;
        }

        

        /// <summary>
        /// Get the entire History for a given Workflow.
        /// </summary>
        /// <param name="workflowId">The id of the Workflow.</param>
        /// <returns></returns>
        [Route("history/{workflowId}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetHistory(string workflowId)
        {
            try
            {
                var toReturn = await _historyLogic.GetHistoryForWorkflow(workflowId);
                await
                    _historyLogic.SaveHistory(new HistoryModel
                    {
                        EventId = "",
                        HttpRequestType = "GET",
                        MethodCalledOnSender = "GetHistory",
                        WorkflowId = workflowId,
                        Message = "Called: GetHistory"
                    });

                return Ok(toReturn);
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel { EventId = "", HttpRequestType = "GET", Message = "Threw: " + e.GetType(), WorkflowId = workflowId, MethodCalledOnSender = "GetHistory" }).Wait();
                return BadRequest("Seems input was not satisfactory");
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveHistory(new HistoryModel { EventId = "", HttpRequestType = "GET", Message = "Threw: " + e.GetType(), WorkflowId = workflowId, MethodCalledOnSender = "GetHistory" }).Wait();
                return NotFound();
            }
            catch (Exception e) 
            {
                _historyLogic.SaveHistory(new HistoryModel {EventId = "", HttpRequestType = "GET", Message = "Threw: " + e.GetType(), WorkflowId = workflowId, MethodCalledOnSender = "GetHistory" }).Wait();
                return InternalServerError(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _historyLogic.Dispose();
            base.Dispose(disposing);
        }
    }
}
