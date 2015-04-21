using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Common.History;
using Event.Interfaces;
using Event.Storage;

namespace Event.Logic
{
    public class EventHistoryLogic : IEventHistoryLogic {
        private readonly IEventStorage _storage;

        public EventHistoryLogic()
        {
            _storage = new EventStorage();
        }

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

        public async Task SaveException(Exception ex, string requestType, string method, string eventId = "", string workflowId = "")
        {
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

        public async Task<IQueryable<HistoryDto>> GetHistoryForEvent(string workflowId, string eventId)
        {
            var models = await _storage.GetHistoryForEvent(workflowId, eventId);
            return models.Select(model => new HistoryDto(model));
        }

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
    }
}