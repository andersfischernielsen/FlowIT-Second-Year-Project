using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Interfaces;
using Event.Models;

namespace Event.Controllers
{
    /// <summary>
    /// EventRulesController handles the incoming requests that modifies this Event's rule-set.
    /// </summary>
    public class EventRulesController : ApiController
    {
        private IEventLogic Logic { get; set; }

        public EventRulesController()
        {
            // Fetches Singleton Logic layer. 
            Logic = EventLogic.GetState();
        }


        /// <summary>
        /// Add a rule to this Event.
        /// </summary>
        /// <param name="id">The id of the calling Event.</param>
        /// <param name="ruleDto">A dto representing the rules which should be between this event and the calling event.</param>
        /// <returns>A task resulting in a Http Result.</returns>
        [Route("event/rules/{id}")]
        [HttpPost]
        public async Task PostRules(string id, [FromBody] EventRuleDto ruleDto)
        {
            // Dismiss request if Event is currently locked
            if (Logic.IsLocked())
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

            // if new entry, add Event to the endPoints-table.
            if (await Logic.KnowsId(id))
            {
                BadRequest(string.Format("{0} already exists!", id));
            }

            await Logic.UpdateRules(id, ruleDto);

            Ok();
        }



        /// <summary>
        /// Updates the rules between this Event and the caller.
        /// </summary>
        /// <param name="id">The id of the calling Event.</param>
        /// <param name="ruleDto">The set of rules, that should exist between caller and receiver.</param>
        /// <returns>A task resulting in a Http Result.</returns>
        [Route("event/rules/{id}")]
        [HttpPut]
        public async Task PutRules(string id, [FromBody] EventRuleDto ruleDto)
        {
            // Dismiss request if Event is currently locked
            if (Logic.IsLocked())
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
            if (!await Logic.KnowsId(id))
            {
                BadRequest(string.Format("{0} does not exist!", id));
            }

            await Logic.UpdateRules(id, ruleDto);

            Ok();
        }


        /// <summary>
        /// Deletes all rules associated with the Event with the given id.
        /// </summary>
        /// <param name="id">The id of the calling Event.</param>
        /// <returns>A task resulting in an Http Result.</returns>
        [Route("event/rules/{id}")]
        [HttpDelete]
        public async Task DeleteRules(string id)
        {
            // Dismiss request if Event is currently locked
            if (Logic.IsLocked())
            {
                // Event is currently locked)
                StatusCode(HttpStatusCode.MethodNotAllowed);
            }

            if (!await Logic.KnowsId(id))
            {
                BadRequest(string.Format("{0} does not exist!", id));
            }

            await Logic.UpdateRules(id,
                // Set all states to false to remove them from storage.
                // This effectively removes all rules associated with the given id.
                // Possibly - to save memory - it could be null instead.
                // Todo: Read above
                new EventRuleDto
                {
                    Condition = false,
                    Exclusion = false,
                    Inclusion = false,
                    Response = false
                });
            // Remove the id because it is no longer associated with any rules.
            await Logic.RemoveIdAndUri(id);

            Ok();
        }

    }
}
