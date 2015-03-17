using System.Collections.Generic;
using Common;

namespace Server
{
    public interface IStorage
    {
        /// <summary>
        /// Get all workflows
        /// </summary>
        /// <returns></returns>
        IList<WorkflowDto> GetWorkflows();
        /// <summary>
        /// Gets all events from a workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        IList<EventAddressDto> GetEvents(WorkflowDto workflow);
        /// <summary>
        /// Add event to workflow
        /// 
        /// Throws some kind of exception if not succesfull, or should this return a bool?
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventDto"></param>
        void AddEventToWorkflow(WorkflowDto workflow, EventAddressDto eventDto);
        /// <summary>
        /// Remove event from workflow
        /// 
        /// Throws some kind of exception if not succesfull, or should this return a bool?
        /// </summary>
        /// <param name="workflow"></param>
        /// <param name="eventDto"></param>
        void RemoveEventFromWorkflow(WorkflowDto workflow, EventAddressDto eventDto);
    }
}
