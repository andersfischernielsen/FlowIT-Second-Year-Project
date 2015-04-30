using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.History;

namespace Event.Interfaces
{
    /// <summary>
    /// IEventHistoryLogic is a logic-layer, that handles logic regarding Event-history. 
    /// </summary>
    public interface IEventHistoryLogic : IDisposable
    {
        /// <summary>
        /// Returns the History for the specified Event. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the specified Event</param>
        /// <returns></returns>
        Task<IEnumerable<HistoryDto>> GetHistoryForEvent(string workflowId, string eventId);

        /// <summary>
        /// Will save a History. Should be used if an operation throws an exception.   
        /// </summary>
        /// <param name="ex">Exception that was thrown</param>
        /// <param name="requestType">HTTP-request-type, i.e. POST, GET, PUT or DELETE</param>
        /// <param name="method">Should identify the method, that makes call to this method</param>
        /// <param name="eventId">Id of the Event, that was involved in the operation that caused the exception</param>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <returns></returns>
        Task SaveException(Exception ex, string requestType, string method, string eventId = "", string workflowId = "");

        /// <summary>
        /// Will save a History to storage. Should be used, when an operation was carried out succesfully.
        /// </summary>
        /// <param name="requestType">HTTP-request-type, i.e. POST, GET, PUT or DELETE</param>
        /// <param name="method">Should identify the method, that makes call to this method</param>
        /// <param name="eventId">>Id of the Event, that was involved in the operation</param>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <returns></returns>
        Task SaveSuccesfullCall(string requestType, string method, string eventId = "", string workflowId = "");
    }
}
