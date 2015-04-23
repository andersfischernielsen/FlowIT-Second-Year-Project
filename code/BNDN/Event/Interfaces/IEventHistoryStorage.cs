using System.Linq;
using System.Threading.Tasks;
using Common.History;

namespace Event.Interfaces
{
    public interface IEventHistoryStorage {
        /// <summary>
        /// Save a given HistoryModel to storage.
        /// </summary>
        /// <param name="toSave"></param>
        Task SaveHistory(HistoryModel toSave);

        /// <summary>
        /// Get every HistoryModel in storage for a given event.
        /// </summary>
        /// <returns></returns>
        Task<IQueryable<HistoryModel>> GetHistoryForEvent(string workflowId, string eventId);
    }
}
