using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Common;

namespace Event.Controllers
{
    [RoutePrefix("event")]
    public class EventController : ApiController
    {
        #region EventEvent
        public async Task<EventDto> Get()
        {
            //Todo
            return await Task.Run(() => new EventDto());
        }

        #region Rules
        [Route("rules")]
        public async Task<IHttpActionResult> PostRules(EventRuleDto ruleDto)
        {
            return BadRequest();
        }

        [Route("rules")]
        public async Task<IHttpActionResult> PutRules(EventRuleDto ruleDto)
        {
            return BadRequest();
        }

        [Route("rules")]
        public async Task<IHttpActionResult> DeleteRules()
        {
            //Find out who called and delete those entries!
            return BadRequest();
        }
        #endregion
        #endregion

        #region ClientEvent

        [Route("state")]
        public async Task<EventStateDto> GetState()
        {
            return await State.EventStateDto;
        }

        [Route("")]
        public async Task<IHttpActionResult> Execute(bool execute)
        {
            if (execute)
            {
                if ((await (State.EventStateDto)).Executable)
                {
                    State.Executed = true;
                    return BadRequest();
                }
                return BadRequest("Not possible to execute event.");
            }
            else
            {
                // Todo: Is this what should happen when execute is false?
                State.Executed = false;
                return Ok();
            }
        }
        #endregion
    }
}