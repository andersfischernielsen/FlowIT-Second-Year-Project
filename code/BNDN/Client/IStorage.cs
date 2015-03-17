using System.Collections.Generic;
using Common;

namespace Client
{
    public interface IStorage
    {
        /// <summary>
        /// Get all workflows
        /// </summary>
        /// <returns></returns>
        IList<WorkflowDto> GetWorkflows();

        /// <summary>
        /// Get all events
        /// </summary>
        /// <returns></returns>
        IList<EventAddressDto> GetEvents();
        /// <summary>
        /// Add event to workflow
        /// 
        /// Throws some kind of exception if not succesfull, or should this return a bool?
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventDto"></param>
        void AddEventToWorkflow(string workflow, EventAddressDto eventDto);
        /// <summary>
        /// Remove event from workflow
        /// 
        /// Throws some kind of exception if not succesfull, or should this return a bool?
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventDto"></param>
        void RemoveEventFromWorkflow(string workflow, EventAddressDto eventDto);
    }
}
