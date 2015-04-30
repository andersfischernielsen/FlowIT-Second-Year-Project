using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.DTO.History;

namespace Server.Interfaces
{
    public interface IWorkflowHistoryLogic : IDisposable
    {
        Task<IEnumerable<HistoryDto>> GetHistoryForWorkflow(string workflowId);
        Task SaveHistory(HistoryModel toSave);
        Task SaveNoneWorkflowSpecificHistory(HistoryModel toSave);
    }
}
