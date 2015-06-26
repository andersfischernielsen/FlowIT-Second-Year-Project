using System;
using System.Threading.Tasks;
using System.Web.Http;
using Event.Interfaces;
using Event.Logic;

namespace Event.Controllers
{
    /// <summary>
    /// HistoryController handles HTTP-request regarding Event History
    /// </summary>
    public class HistoryController : ApiController
    {
        private readonly IEventHistoryLogic _historyLogic;

        /// <summary>
        /// Constructor used for dependency-injection
        /// </summary>
        /// <param name="historyLogic">Logic-layer implementing the IEventHistory interface</param>
        public HistoryController(IEventHistoryLogic historyLogic)
        {
            if (historyLogic == null)
            {
                throw new ArgumentNullException("historyLogic");
            }
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
        public async Task<IHttpActionResult> GetHistory(string workflowId, string eventId)
        {
            try
            {
                var toReturn = await _historyLogic.GetHistoryForEvent(workflowId, eventId);
                await _historyLogic.SaveSuccesfullCall("GET", "GetHistory", eventId, workflowId);

                return Ok(toReturn);
            }

            catch (Exception e)
            {
                _historyLogic.SaveException(e, "GET", "GetHistory", eventId, workflowId);

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
