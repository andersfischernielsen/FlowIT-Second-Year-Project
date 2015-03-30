using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client
{
    public class EventConnection : IEventConnection
    {
        private readonly HttpClientToolbox _http;
        private readonly EventAddressDto _eventDto;
        public static Dictionary<string, IList<string>> RoleForWorkflow { get; set; }

        public EventConnection(EventAddressDto eventDto)
        {
            this._eventDto = eventDto;
            _http = new HttpClientToolbox(_eventDto.Uri);
        }

        public async Task<EventStateDto> GetState()
        {
            return await _http.Read<EventStateDto>("event/state/-1");
        }
        

        public async Task Execute(bool b, string workflowId)
        {
            IList<string> roles;
            RoleForWorkflow.TryGetValue(workflowId, out roles);

            await _http.Update("event/executed", new ExecuteDto { Roles = roles });
        }
    }
}
