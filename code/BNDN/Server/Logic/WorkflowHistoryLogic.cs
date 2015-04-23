using System.Linq;
using System.Threading.Tasks;
using Common.History;
using Server.Interfaces;
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

        public async Task SaveNoneWorkflowSpecificHistory(HistoryModel toSave)
        {
            await _storage.SaveNonWorkflowSpecificHistory(toSave);
        }
    }
}