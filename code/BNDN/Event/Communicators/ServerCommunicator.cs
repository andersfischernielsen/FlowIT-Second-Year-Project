using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;

namespace Event.Communicators
{
    /// <summary>
    /// ServerCommunicator is the module through which Event has its outgoing communication with Server 
    /// ServerCommunicator implements the IServerFromEvent interface
    /// </summary>
    public class ServerCommunicator : IServerFromEvent
    {
        private readonly HttpClientToolbox _httpClient;
        private readonly string _serverBaseAddress;
        
        // _eventId represents this Event's id, and _workflowId the workflow that this Event is a part of.
        private readonly string _eventId;
        private readonly string _workflowId;


        public ServerCommunicator(String baseAddress, string eventId, string workFlowId)
        {
            _workflowId = workFlowId;
             _eventId = eventId;
            _serverBaseAddress = baseAddress;
            _httpClient = new HttpClientToolbox(_serverBaseAddress);
        }


        public async Task<IEnumerable<EventAddressDto>> PostEventToServer(EventAddressDto addressDto)
        {
            if (string.IsNullOrEmpty(_eventId)
                    || string.IsNullOrEmpty(_workflowId)
                    || string.IsNullOrEmpty(_serverBaseAddress))
            {
                throw new InvalidOperationException("EventId, workflowId and serverBaseAddress must be non-null");
            }

            var path = string.Format("workflows/{0}", _workflowId);
            return await _httpClient.Create<EventAddressDto, IEnumerable<EventAddressDto>>(path, addressDto);
        }

        public async Task DeleteEventFromServer()
        {
            var path = string.Format("workflows/{0}/{1}", _workflowId, _eventId);
            await _httpClient.Delete(path);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}