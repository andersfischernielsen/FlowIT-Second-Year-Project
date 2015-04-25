using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.History;
using Server.Interfaces;
using Server.Storage;

namespace Server.Logic
{
    /// <summary>
    /// Logic layer that handles operations related to history ('logging')
    /// </summary>
    public class WorkflowHistoryLogic : IWorkflowHistoryLogic
    {
        private readonly IServerStorage _storage;

        /// <summary>
        /// Default constructor. 
        /// </summary>
        public WorkflowHistoryLogic()
        {
            _storage = new ServerStorage();
        }

        /// <summary>
        /// Returns the Server-history for the specified workflow. 
        /// </summary>
        /// <param name="workflowId">Id of the workflow, whose history is to be obtained</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null</exception>
        public async Task<IEnumerable<HistoryDto>> GetHistoryForWorkflow(string workflowId)
        {
            if (workflowId == null)
            {
                throw new ArgumentNullException();
            }

            var models = (await _storage.GetHistoryForWorkflow(workflowId)).ToList();
            return models.Select(model => new HistoryDto(model));
        }

        /// <summary>
        /// Saves the history given in the provided toSave.
        /// </summary>
        /// <param name="toSave">Contains the information about the history that should be saved</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null</exception>
        public async Task SaveHistory(HistoryModel toSave)
        {
            if (toSave == null)
            {
                throw new ArgumentNullException();
            }

            await _storage.SaveHistory(toSave);
        }

        /// <summary>
        /// Saves a history that is non-specific to a workflow. 
        /// </summary>
        /// <param name="toSave">Information to be saved</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if the argument is null</exception>
        public async Task SaveNoneWorkflowSpecificHistory(HistoryModel toSave)
        {
            if (toSave == null)
            {
                throw new ArgumentNullException();
            }

            await _storage.SaveNonWorkflowSpecificHistory(toSave);
        }
    }
}