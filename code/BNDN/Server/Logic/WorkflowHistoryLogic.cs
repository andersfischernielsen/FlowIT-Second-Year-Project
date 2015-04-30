using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.DTO.History;
using Server.Interfaces;
using Server.Storage;

namespace Server.Logic
{
    /// <summary>
    /// Logic layer that handles operations related to history ('logging')
    /// </summary>
    public class WorkflowHistoryLogic : IWorkflowHistoryLogic
    {
        private readonly IServerHistoryStorage _storage;

        /// <summary>
        /// Default constructor. 
        /// </summary>
        public WorkflowHistoryLogic()
        {
            _storage = new ServerStorage();
        }

        public async Task<IEnumerable<HistoryDto>> GetHistoryForWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            var models = (await _storage.GetHistoryForWorkflow(workflowId)).ToList();
            return models.Select(model => new HistoryDto(model));
        }

        public async Task SaveHistory(HistoryModel toSave)
        {
            if (toSave == null)
            {
                throw new ArgumentNullException();
            }

            await _storage.SaveHistory(toSave);
        }

        public async Task SaveNoneWorkflowSpecificHistory(HistoryModel toSave)
        {
            if (toSave == null)
            {
                throw new ArgumentNullException();
            }

            await _storage.SaveNonWorkflowSpecificHistory(toSave);
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}