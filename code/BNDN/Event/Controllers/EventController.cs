using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web.Http;
using Common;

namespace Event.Controllers
{
    [RoutePrefix("event")]
    public class EventController : ApiController
    {
        public InMemoryStorage InMemoryStorage { get; set; }
        public EventController()
        {
            InMemoryStorage = InMemoryStorage.GetState();
        }

        #region EventEvent
        [HttpGet]
        public async Task<EventDto> Get()
        {
            return await InMemoryStorage.EventDto;
        }

        #region Rules
        [Route("rules/{id}")]
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
            if (InMemoryStorage.KnowsId(id))
            {
                return BadRequest(string.Format("{0} already exists!", id));
            }
            var addresses = (await Dns.GetHostAddressesAsync(Request.RequestUri.Host)).Where(address => address.AddressFamily == AddressFamily.InterNetwork).ToArray();
            if (!addresses.Any() || addresses.Length > 1)
            {
                throw new Exception("Bad address!" + addresses.Length);
            }

            var endPoint = new IPEndPoint(addresses[0], Request.RequestUri.Port);
            InMemoryStorage.RegisterIdWithEndPoint(id, endPoint);

            InMemoryStorage.UpdateRules(id, ruleDto);

            return Ok();
        }

        [Route("rules/{id}")]
        [HttpPut]
        public async Task<IHttpActionResult> PutRules(string id, [FromBody] EventRuleDto ruleDto)
        {
            // if new entry, add Event to the endPoints-table.
            if (!InMemoryStorage.KnowsId(id))
            {
                return BadRequest(string.Format("{0} does not exist!", id));
            }

            InMemoryStorage.UpdateRules(id, ruleDto);

            return Ok();
        }

        [Route("rules/{id}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteRules(string id)
        {
            if (!InMemoryStorage.KnowsId(id))
            {
                return BadRequest(string.Format("{0} does not exist!", id));
            }
            await Task.Run(() =>
            {
                InMemoryStorage.UpdateRules(id,
                    // Set all states to false to remove them from storage.
                    new EventRuleDto
                    {
                        Condition = false,
                        Exclusion = false,
                        Inclusion = false,
                        Response = false
                    });
                InMemoryStorage.RemoveIdAndEndPoint(id);
            });
            return Ok();
        }
        #endregion
        #endregion

        #region ClientEvent

        [Route("state")]
        [HttpGet]
        public async Task<EventStateDto> GetState()
        {
            return await InMemoryStorage.EventStateDto;
        }

        [HttpPut]
        public async Task<IHttpActionResult> Execute(bool execute)
        {
            if (execute)
            {
                if ((await (InMemoryStorage.EventStateDto)).Executable)
                {
                    InMemoryStorage.Executed = true;
                    return BadRequest();
                }
                return BadRequest("Not possible to execute event.");
            }
            else
            {
                // Todo: Is this what should happen when execute is false?
                InMemoryStorage.Executed = false;
                return Ok();
            }
        }
        #endregion
    }
}