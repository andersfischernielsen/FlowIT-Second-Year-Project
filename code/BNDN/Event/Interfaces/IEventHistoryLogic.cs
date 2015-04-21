using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.History;

namespace Event.Interfaces
{
    public interface IEventHistoryLogic 
    {
        Task SaveHistory(HistoryModel toSave);

        Task<IQueryable<HistoryModel>> GetHistoryForEvent(string workflowId, string eventId);
    }
}
