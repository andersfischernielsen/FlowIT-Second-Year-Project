﻿using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Event.Interfaces;
using Event.Models;

namespace Event.Controllers
{
    /// <summary>
    /// EventRulesController handles the incoming requests that modifies this Event's rules-set.
    /// </summary>
    public class EventRulesController : ApiController
    {
        private IEventLogic Logic { get; set; }

        public EventRulesController()
        {
            // Fetches Singleton-storage
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
        public async Task<IHttpActionResult> PostRules(string id, [FromBody] EventRuleDto ruleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (ruleDto == null)
            {
                return BadRequest("Request requires data");
            }

            // if new entry, add Event to the endPoints-table.
            if (await Logic.KnowsId(id))
            {
                return BadRequest(string.Format("{0} already exists!", id));
            }

            await Logic.UpdateRules(id, ruleDto);

            return Ok();
        }



        /// <summary>
        /// Updates the rules between this Event and the caller.
        /// </summary>
        /// <param name="id">The id of the calling Event.</param>
        /// <param name="ruleDto">The complete set of updated rules. 
        /// If existing rules are not in the dto they will be removed.</param>
        /// <returns>A task resulting in a Http Result.</returns>
        [Route("event/rules/{id}")]
        [HttpPut]
        public async Task<IHttpActionResult> PutRules(string id, [FromBody] EventRuleDto ruleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (ruleDto == null)
            {
                return BadRequest("Request requires data");
            }

            // If the id is not known to this event, the PUT-call shall fail!
            if (!await Logic.KnowsId(id))
            {
                return BadRequest(string.Format("{0} does not exist!", id));
            }

            await Logic.UpdateRules(id, ruleDto);

            return Ok();
        }


        /// <summary>
        /// Deletes all rules associated with the Event with the given id.
        /// </summary>
        /// <param name="id">The id of the calling Event.</param>
        /// <returns>A task resulting in an Http Result.</returns>
        [Route("event/rules/{id}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteRules(string id)
        {
            if (!await Logic.KnowsId(id))
            {
                return BadRequest(string.Format("{0} does not exist!", id));
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

            return Ok();
        }

    }
}
