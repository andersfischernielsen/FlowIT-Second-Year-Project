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
        private readonly string _workflowId;

        /// <summary>
        /// This constructor is used forwhen the connection should have knowlegde about roles.
        /// </summary>
        /// <param name="eventDto"></param>
        /// <param name="workflowId"></param>
        public EventConnection(EventAddressDto eventDto, string workflowId)
            : this(new HttpClientToolbox(eventDto.Uri))
        {
            _eventDto = eventDto;
            _workflowId = workflowId;
        }


        /// <summary>
        /// For testing purposes (dependency injection of mocked Toolbox).
        /// </summary>
        /// <param name="toolbox"></param>
        public EventConnection(HttpClientToolbox toolbox)
        {
            _httpClient = toolbox;
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

        public async Task Execute(bool b)
        {
            IList<string> roles;
            LoginViewModel.RoleForWorkflow.TryGetValue(_workflowId, out roles);

            var eventId = _eventDto.Id;
            await _httpClient.Update(String.Format("events/{0}/executed/", eventId), new ExecuteDto { Roles = roles });
        }
    }
}
