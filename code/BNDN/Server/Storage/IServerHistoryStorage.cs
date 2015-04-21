using System.Linq;
using System.Threading.Tasks;
using Common.History;

namespace Server.Storage
{
    public interface IServerHistoryStorage
    {
        /// <summary>
        /// Get every HistoryModel in storage for a given workflow.
        /// </summary>
        /// <returns></returns>
        Task<IQueryable<HistoryModel>> GetHistoryForWorkflow(string workflowId);
    }
}
