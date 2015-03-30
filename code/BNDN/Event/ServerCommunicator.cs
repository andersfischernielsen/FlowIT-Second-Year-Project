using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Event.Interfaces;

namespace Event
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

        
        /// <summary>
        /// Notify the Server of the existence of this Event. This method should be called after the Event has received its
        /// state (as JSON) after being instantiated. Otherwise it isn't possible to tell the Server what Workflow this
        /// Event should be part of.
        /// </summary>
        public async void NotifyServerOfExistence()
        {
            //Event won't do this unless it's been properly initialised.
            if (string.IsNullOrEmpty(_eventId) 
                    || string.IsNullOrEmpty(_workflowId) 
                    || string.IsNullOrEmpty(_serverBaseAddress)) {
                return;
            }

            var path = _serverBaseAddress + "Workflows/" + _workflowId;
            await _httpClient.Create(path, new EventAddressDto {Id = _eventId, Uri = new Uri(_serverBaseAddress)});
        }


        /**
         * GetWorkFlowEvents returns a Task, that - if successfull - holds a List of EventAddressDto
         * representing the events in the requested workflow. 
         */
        public async Task<IList<EventAddressDto>> GetWorkFlowEvents(int workflowId)
        {
            try
            {
                var path = "workflows/" + workflowId;
                var a = await _httpClient.ReadList<EventAddressDto>(path);
                return a;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /**
         * SendHeartbeatToServer communicates to Server, that this Event is alive.
         * The method will rely on the eventId that was provided during initialization
         */
        // TODO: Discuss with Server-implemter what is expected (on ServerSide) to be contained within HeartbeatDto
        public async void SendHeartbeatToServer()
        {
            var path = _workflowId + "/" + _eventId;
            var heartBeatDto = new HeartBeatDto {EventId = _eventId};
            await _httpClient.Create(path, heartBeatDto);
        }

        /**
         * RequestDeletionOfEventAtServer will inform Server that an event with id eventToBeDeletedId
         * should be deleted (typically because the event has been detected as being 'dead')
         */
        public async Task RequestDeletionOfEventAtServer(string eventToBeDeletedId)
        {
            var path = String.Format("workflows/{0}/{1}", _workflowId, eventToBeDeletedId);
            await _httpClient.Delete(path);
        }

        public async Task<IEnumerable<EventAddressDto>> PostEventToServer(EventAddressDto addressDto)
        {
            var path = string.Format("workflows/{0}", _workflowId);
            return await _httpClient.Create<EventAddressDto, IEnumerable<EventAddressDto>>(path, addressDto);
        }

        public async Task DeleteEventFromServer()
        {
            var path = string.Format("workflows/{0}/{1}", _workflowId, _eventId);
            await _httpClient.Delete(path);
        }
    }
}