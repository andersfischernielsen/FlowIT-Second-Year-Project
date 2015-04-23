﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Common.History;
using Server.Logic;

namespace Server.Controllers {
    public class HistoryController : ApiController {
        private readonly IWorkflowHistoryLogic _historyLogic;

        public HistoryController()
        {
            _historyLogic = new WorkflowHistoryLogic();
        }

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
        public async Task<IEnumerable<HistoryDto>> GetHistory(string workflowId)
        {
            try {
                var toReturn = await _historyLogic.GetHistoryForWorkflow(workflowId);
                await _historyLogic.SaveHistory(new HistoryModel {EventId = "", HttpRequestType = "GET", MethodCalledOnSender = "GetHistory", WorkflowId = workflowId, Message = "Called: GetHistory" });

                return toReturn;
            }

            catch (Exception e) {
                _historyLogic.SaveHistory(new HistoryModel {EventId = "", HttpRequestType = "GET", Message = "Threw: " + e.GetType(), WorkflowId = workflowId, MethodCalledOnSender = "GetHistory" });

                throw;
            }
        }
    }
}
