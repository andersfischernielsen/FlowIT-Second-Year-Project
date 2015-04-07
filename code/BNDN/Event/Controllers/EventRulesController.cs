using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Models;
using Event.Storage;

namespace Event.Controllers
{
    /// <summary>
    /// EventRulesController handles the incoming requests that modifies this Event's rule-set.
    /// </summary>
    public class EventRulesController : ApiController
    {

        /// <summary>
        /// Add a rule to this Event.
        /// </summary>
        /// <param name="senderId">The id of the calling Event.</param>
        /// <param name="ruleDto">A dto representing the rules which should be between this event and the calling event.</param>
        /// <param name="eventId">The id of the event, that sender wants to add rules to</param>
        /// <returns>A task resulting in a Http Result.</returns>
        [Route("events/{eventId}/rules/{senderId}")]
        [HttpPost]
        public async Task PostRules(string senderId, [FromBody] EventRuleDto ruleDto, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Dismiss request if Event is currently locked
                if (logic.IsLocked())
                {
                    // Event is currently locked)
                    StatusCode(HttpStatusCode.MethodNotAllowed);
                }

                // Check that provided input can be mapped onto a legal instance of EventRuleDto
                if (!ModelState.IsValid)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Provided input could not be mapped onto an instance of EventRuleDto"));
                }
                if (ruleDto == null)
                {
                    // TODO: Discuss: This check should be obsolete by now, eith above !ModelState.IsValid check
                    BadRequest("Request requires data");
                }

                // TODO: Discuss: Are we sure about the following logic? Seems fishy
                // if new entry, add Event to the endPoints-table.
                //if (await logic.KnowsId(senderId))
                //{
                //    BadRequest(string.Format("{0} already exists!", senderId));
                //}

                await logic.UpdateRules(senderId, ruleDto);
            }
        }



        /// <summary>
        /// Updates the rules between this Event and the caller.
        /// </summary>
        /// <param name="senderId">The id of the calling Event.</param>
        /// <param name="ruleDto">The set of rules, that should exist between caller and receiver.</param>
        /// <param name="eventId">The id of the Event whose rules are to be updated</param>
        /// <returns>A task resulting in a Http Result.</returns>
        [Route("events/{eventId}/rules/{senderId}")]
        [HttpPut]
        public async Task PutRules(string senderId, [FromBody] EventRuleDto ruleDto, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Dismiss request if Event is currently locked
                if (logic.IsLocked())
                {
                    // Event is currently locked)
                    StatusCode(HttpStatusCode.MethodNotAllowed);
                }

                if (!ModelState.IsValid)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Provided input could not be mapped onto an instance of EvemtRuleDto"));
                }
                if (ruleDto == null)
                {
                    // TODO: Discuss: This check should be obsolete by now, eith above !ModelState.IsValid check. Consider deleting
                    BadRequest("Request requires data");
                }

                // If the id is not known to this event, the PUT-call shall fail!
                //if (!await logic.KnowsId(senderId))
                //{
                //    BadRequest(string.Format("{0} does not exist!", senderId));
                //}

                await logic.UpdateRules(senderId, ruleDto);
            }
        }


        /// <summary>
        /// Deletes all rules associated with the Event with the given id.
        /// </summary>
        /// <param name="senderId">The id of the calling Event.</param>
        /// <param name="eventId">Id of the event that sender wants to delete rules at</param>
        /// <returns>A task resulting in an Http Result.</returns>
        [Route("events/{eventId}/rules/{senderId}")]
        [HttpDelete]
        public async Task DeleteRules(string senderId, string eventId)
        {
            using (var logic = new EventLogic(eventId))
            {
                // Dismiss request if Event is currently locked
                if (logic.IsLocked())
                {
                    // Event is currently locked)
                    StatusCode(HttpStatusCode.MethodNotAllowed);
                }

                //if (!await logic.KnowsId(senderId))
                //{
                //    BadRequest(string.Format("{0} does not exist!", senderId));
                //}

                // TODO: Consider: Is this way of deleting still legit?
                await logic.UpdateRules(senderId,
                    // Set all states to false to remove them from storage.
                    // This effectively removes all rules associated with the given id.
                    // Possibly - to save memory - it could be null instead.
                    // Todo: Read above
                    new EventRuleDto
                    {
                        Id = senderId,
                        Condition = false,
                        Exclusion = false,
                        Inclusion = false,
                        Response = false
                    });
                // Remove the id because it is no longer associated with any rules.
                //await logic.RemoveIdAndUri(senderId);
            }
        }

    }
}
