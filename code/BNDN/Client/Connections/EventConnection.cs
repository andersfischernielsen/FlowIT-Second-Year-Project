using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.ViewModels;
using Common;

namespace Client.Connections
{
    public class EventConnection : IEventConnection
    {
        private readonly HttpClientToolbox _httpClient;
        private readonly EventAddressDto _eventDto;

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
        public async Task ResetEvent()
        {
            var eventId = _eventDto.Id;
            await _httpClient.Update(String.Format("events/{0}/reset", eventId), new EventDto{Name = "ResetDTO - DO NOT USE"});
        }
        

        public async Task Execute(bool b, string workflowId)
        {
            IList<string> roles;
            LoginViewModel.RoleForWorkflow.TryGetValue(workflowId, out roles);

            var eventId = _eventDto.Id;
            await _httpClient.Update(String.Format("events/{0}/executed/",eventId),new ExecuteDto{Roles = roles});
        }
    }
}
