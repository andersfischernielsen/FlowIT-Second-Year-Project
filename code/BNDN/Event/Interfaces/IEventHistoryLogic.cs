using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.DTO.History;

namespace Event.Interfaces
{
    public interface IEventHistoryLogic : IDisposable
    {
        Task<IEnumerable<HistoryDto>> GetHistoryForEvent(string workflowId, string eventId);

        Task SaveException(Exception ex, string requestType, string method, string eventId = "", string workflowId = "");

        Task SaveSuccesfullCall(string requestType, string method, string eventId = "", string workflowId = "");
    }
}
