using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Common;
using Common.History;
using Server.Storage;

namespace Server.Logic
{
    public class WorkflowHistoryLogic : IWorkflowHistoryLogic
    {
        private readonly IServerStorage _storage;

        public WorkflowHistoryLogic()
        {
            _storage = new ServerStorage();
        }

        public async Task<IQueryable<HistoryDto>> GetHistoryForWorkflow(string workflowId)
        {
            var models = await _storage.GetHistoryForWorkflow(workflowId);
            return models.Select(model => new HistoryDto(model));
        }

        public async Task SaveHistory(HistoryModel toSave)
        {
            await _storage.SaveHistory(toSave);
        }
    }
}