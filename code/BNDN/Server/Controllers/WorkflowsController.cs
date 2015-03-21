using System.Collections.Generic;
using System.Web.Http;
using Common;
using Server.Interfaces;
using Server.Models;

namespace Server.Controllers
{
    public class WorkflowsController : ApiController
    {
        private IServerStorage Storage { get; set; }

        public WorkflowsController()
        {
            Storage = new WorkflowStorage();
        }


        /// <summary>
        /// Returns a list of all workflows currently held at this Server
        /// </summary>
        /// <returns>List of WorkflowDto</returns>
        public IEnumerable<WorkflowDto> Get()
        {
            return Storage.GetAllWorkFlows();
        }


        // GET: api/Workflows/5
        /// <summary>
        /// Given an workflowId, this method returns all events within that workflow
        /// </summary>
        /// <param name="workFlowid">Id of the requested workflow</param>
        /// <returns>IEnumerable of EventAddressDto</returns>
        public IEnumerable<EventAddressDto> Get(int workFlowid)
        {
            return Storage.GetEventsWithinWorkflow(workFlowid);
        }


        /// <summary>
        /// PostEventToWorkFlow adds an Event to a workflow with the specified workflowid. 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="workflowId"></param>
        /// <param name="eventToAddDto"></param>
        [Route("Workflows/{workflowId}/{eventId}")]
        [HttpPost]
        // TODO: Clarify what information should Event provide to Server, when submitting itself to Server?
        // TODO: How does Event know that an eventId is not already taken?
        public void PostEventToWorkFlow(string eventId, string workflowId, [FromBody] EventDto eventToAddDto)
        {
            // Extract id's
            int eventID = ExtractNumberFromString(eventId);
            int workflowID = ExtractNumberFromString(workflowId);

            // Add this Event to the specified workflow
            Storage.AddEventToWorkflow(workflowID,eventID,eventToAddDto);
        }

        
        [Route("Workflows/workflowId/eventId/")]
        [HttpDelete]
        public void DeleteEventFromWorkflow(string workflowId, string eventId)
        {
            // Extract the ID's
            int eventID = ExtractNumberFromString(eventId);
            int workflowID = ExtractNumberFromString(workflowId);

            // Delete the given event id from the list of workflow-events.
            Storage.RemoveEventFromWorkFlow(eventID,workflowID);
        }


        private int ExtractNumberFromString(string input)
        {
            // TODO: Exception handling during parsing. 
            int returnValue = -1;
            int.TryParse(input, out returnValue);

            return returnValue;
        }
    }
}
