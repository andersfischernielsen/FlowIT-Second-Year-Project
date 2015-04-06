using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Client
{
    public class EventConnection : IEventConnection
    {
        private readonly HttpClientToolbox _httpClient;
        private readonly EventAddressDto _eventDto;
        public static Dictionary<string, IList<string>> RoleForWorkflow { get; set; }

        public EventConnection(EventAddressDto eventDto)
        {
            _eventDto = eventDto;
            _httpClient = new HttpClientToolbox(_eventDto.Uri);
        }

        public async Task<EventStateDto> GetState()
        {
            var eventId = _eventDto.Id;
            return await _httpClient.Read<EventStateDto>(String.Format("events/{0}/state/-1", eventId));   
        }
        

        public async Task Execute(bool b, string workflowId)
        {
            IList<string> roles;
            RoleForWorkflow.TryGetValue(workflowId, out roles);

            var eventId = _eventDto.Id;
            await _httpClient.Update(String.Format("events/{0}/executed/{1}",eventId,b),new ExecuteDto{Roles = roles});
        }
    }
}
