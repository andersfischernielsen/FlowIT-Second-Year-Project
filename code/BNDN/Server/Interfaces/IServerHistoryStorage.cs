using System;
using System.Linq;
using System.Threading.Tasks;
using Common.History;

namespace Server.Interfaces
{
    public interface IServerHistoryStorage : IDisposable
    {
        /// <summary>
        /// Save a given HistoryModel to storage.
        /// </summary>
        /// <param name="toSave"></param>
        Task SaveHistory(HistoryModel toSave);

        Task SaveNonWorkflowSpecificHistory(HistoryModel toSave);

        /// <summary>
        /// Get every HistoryModel in storage for a given workflow.
        /// </summary>
        /// <returns></returns>
        Task<IQueryable<HistoryModel>> GetHistoryForWorkflow(string workflowId);
    }
}
