using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using Common;
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
        // GET: /workflows
        [Route("workflows")]
        public IEnumerable<WorkflowDto> Get()
        {
            return Storage.GetAllWorkflows();
        }


        // GET: /Workflows/5
        /// <summary>
        /// Given an workflowId, this method returns all events within that workflow
        /// </summary>
        /// <param name="workflowId">Id of the requested workflow</param>
        /// <returns>IEnumerable of EventAddressDto</returns>
        [Route("workflows/{workflowId}")]
        [HttpGet]
        public IEnumerable<EventAddressDto> Get(string workflowId)
        {
            Debug.WriteLine("Hmm, we got here!");
            return Storage.GetEventsWithinWorkflow(workflowId);
        }


        /// <summary>
        /// PostEventToWorkFlow adds an Event to a workflow with the specified workflowid. 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="workflowId"></param>
        /// <param name="eventToAddDto"></param>
        [Route("Workflows/{workflowId}")]
        [HttpPost]
        // TODO: Clarify what information should Event provide to Server, when submitting itself to Server?
        // TODO: How does an Event know that an eventId is not already taken?
        public void PostEventToWorkFlow(string workflowId, [FromBody] EventAddressDto eventToAddDto)
        {
            // Add this Event to the specified workflow
            Storage.AddEventToWorkflow(workflowId,eventToAddDto);
        }

        
        [Route("Workflows/{workflowId}/{eventId}")]
        [HttpDelete]
        // TODO: Is there any need to supply more than workflowId and eventId of the event that is to be removed?
        public void DeleteEventFromWorkflow(string workflowId, string eventId)
        {
            // Delete the given event id from the list of workflow-events.
            Debug.WriteLine("Yep, we got here!");
            Storage.RemoveEventFromWorkflow(workflowId,eventId);
        }
    }
}
