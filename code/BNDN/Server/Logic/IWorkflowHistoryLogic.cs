using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.History;

namespace Server.Logic
{
    public interface IWorkflowHistoryLogic
    {
        Task<IQueryable<HistoryDto>> GetHistoryForWorkflow(string workflowId);
        Task SaveHistory(HistoryModel toSave);
    }
}
