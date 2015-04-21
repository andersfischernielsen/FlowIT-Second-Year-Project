using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Common.History;
using Event.Interfaces;

namespace Event.Logic
{
    public class EventHistoryLogic : IEventHistoryLogic {
        private readonly IEventStorage _storage;

        public EventHistoryLogic(IEventStorage storage)
        {
            _storage = storage;
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

        public async Task<IQueryable<HistoryModel>> GetHistoryForEvent(string workflowId, string eventId)
        {
            return await _storage.GetHistoryForEvent(workflowId, eventId);
        }
    }
}