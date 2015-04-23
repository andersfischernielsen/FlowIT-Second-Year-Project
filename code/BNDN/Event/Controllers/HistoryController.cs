using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Common.History;
using Event.Interfaces;
using Event.Logic;

namespace Event.Controllers {
    public class HistoryController : ApiController {
        private readonly IEventHistoryLogic _historyLogic;

        public HistoryController()
        {
            _historyLogic = new EventHistoryLogic();
        }

        public HistoryController(IEventHistoryLogic historyLogic)
        {
            _historyLogic = historyLogic;
        }

        

        /// <summary>
        /// Get the entire History for a given Event.
        /// </summary>
        /// <param name="workflowId">The id of the Workflow in which the Event exists</param>
        /// <param name="eventId">The id of the Event, that you wish to get the history for.</param>
        /// <returns></returns>
        [Route("history/{workflowId}/{eventId}")]
        [HttpGet]
        public async Task<IEnumerable<HistoryDto>> GetHistory(string workflowId, string eventId)
        {
            try {
                var toReturn = await _historyLogic.GetHistoryForEvent(workflowId, eventId);
                await _historyLogic.SaveSuccesfullCall("GET", "GetHistory", eventId, workflowId);

                return toReturn;
            }

            catch (Exception e) {
                _historyLogic.SaveException(e, "GET", "GetHistory", eventId, workflowId);

                throw;
            }
        }
    }
}
