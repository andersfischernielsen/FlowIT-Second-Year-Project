using System.Collections.Generic;
using Common.History;

namespace Server.Logic
{
    interface IWorkflowHistoryLogic
    {
        IList<HistoryModel> GetHistoryForWorkflow(string workflowId);
    }
}
