using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.History;

namespace Server.Interfaces
{
    public interface IWorkflowHistoryLogic
    {
        Task<IEnumerable<HistoryDto>> GetHistoryForWorkflow(string workflowId);
        Task SaveHistory(HistoryModel toSave);
        Task SaveNoneWorkflowSpecificHistory(HistoryModel toSave);
    }
}
