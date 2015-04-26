using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.History;
using Event.Interfaces;
using Event.Storage;

namespace Event.Logic
{
    /// <summary>
    /// EventHistoryLogic is a logic-layer, that handles logic regarding Event-history. 
    /// </summary>
    public class EventHistoryLogic : IEventHistoryLogic 
    {
        private readonly IEventStorage _storage;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EventHistoryLogic()
        {
            _storage = new EventStorage();
        }


        // TODO: Is this method ever used?
        /// <summary>
        /// Will save a History. Should be used for successfull operations in an Event.
        /// </summary>
        /// <param name="toSave">History that needs to be saved</param>
        /// <returns></returns>
        public Task SaveHistory(HistoryModel toSave)
        {
            var asDto = new HistoryModel
            {
                EventId = toSave.EventId,
                HttpRequestType = toSave.HttpRequestType,
                Message = toSave.Message,
                MethodCalledOnSender = toSave.MethodCalledOnSender,
                WorkflowId = toSave.WorkflowId
            };

            return _storage.SaveHistory(asDto);
        }

        /// <summary>
        /// Will save a History. Should be used if an operation throws an exception.   
        /// </summary>
        /// <param name="ex">Exception that was thrown</param>
        /// <param name="requestType">HTTP-request-type, i.e. POST, GET, PUT or DELETE</param>
        /// <param name="method">Should identify the method, that makes call to this method</param>
        /// <param name="eventId">Id of the Event, that was involved in the operation that caused the exception</param>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <returns></returns>
        public async Task SaveException(Exception ex, string requestType, string method, string eventId = "", string workflowId = "")
        {
            //Don't save a null reference.
            if (ex == null) return;

            var toSave = new HistoryModel
            {
                EventId = eventId,
                HttpRequestType = requestType,
                Message = "Threw: " + ex.GetType(),
                MethodCalledOnSender = method,
                WorkflowId = workflowId
            };

            await _storage.SaveHistory(toSave);
        }

        /// <summary>
        /// Returns the History for the specified Event. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <param name="eventId">Id of the specified Event</param>
        /// <returns></returns>
        public async Task<IEnumerable<HistoryDto>> GetHistoryForEvent(string workflowId, string eventId)
        {
            var models = (await _storage.GetHistoryForEvent(workflowId, eventId)).ToList();
            return models.Select(model => new HistoryDto(model));
        }

        /// <summary>
        /// Will save a History to storage. Should be used, when an operation was carried out succesfully.
        /// </summary>
        /// <param name="requestType">HTTP-request-type, i.e. POST, GET, PUT or DELETE</param>
        /// <param name="method">Should identify the method, that makes call to this method</param>
        /// <param name="eventId">>Id of the Event, that was involved in the operation</param>
        /// <param name="workflowId">Id of the workflow, the Event belongs to</param>
        /// <returns></returns>
        public async Task SaveSuccesfullCall(string requestType, string method, string eventId = "", string workflowId = "")
        {
            var toSave = new HistoryModel
            {
                EventId = eventId,
                HttpRequestType = requestType,
                Message = "Succesfully called: " + method,
                MethodCalledOnSender = method,
                WorkflowId = workflowId
            };

            await _storage.SaveHistory(toSave);
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}