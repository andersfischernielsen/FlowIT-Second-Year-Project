using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Common;
using Event.Controllers;
using Event.Interfaces;
using Event.Models;

namespace Event
{
    /// <summary>
    /// ServerCommunicator is the module through which Event has its outgoing communication with Server 
    /// ServerCommunicator implements the IServerFromEvent interface
    /// </summary>
    public class ServerCommunicator : IServerFromEvent
    {
        private HttpClientToolbox _httpClient;
        private string _serverBaseAddress;
        
        // _eventId represents this Event's id, and _workflowId the workflow that this Event is a part of
        private int _eventId;
        private int _workflowId;


        public ServerCommunicator(String baseAddress, int eventId, int workFlowId)
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
            if (!_eventId.HasValue || !_workflowId.HasValue || string.IsNullOrEmpty(_serverBaseAddress)) {
                return;
            }

            try {
                var path = _serverBaseAddress + "Workflows/" + _workflowId;
                await _httpClient.Create(path, new EventAddressDto {Id = _eventId.Value, Uri = new Uri(_serverBaseAddress)});
            }
            catch (Exception ex) {
                //TODO: Server is down? What do then?
                throw;
            }
        }


        /**
         * GetWorkFlowEvents returns a Task, that - if successfull - holds a List of EventAddressDto
         * representing the events in the requested workflow. 
         */
        // TODO: Exception-handling? Is that handled by Wind's HttpClient?
        public async Task<IList<EventAddressDto>> GetWorkFlowEvents(int workflowId)
        {
            try
            {
                var path = _serverBaseAddress + "Workflows/" + workflowId;
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
        // TODO: Exception-handling?
        public async void SendHeartbeatToServer()
        {
            string path = _workflowId + "/" + _eventId;
            var heartBeatDto = new HeartBeatDto().EventId = _eventId;
            try
            {
                await _httpClient.Create(path, heartBeatDto);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /**
         * RequestDeletionOfEventAtServer will inform Server that an event with id eventToBeDeletedId
         * should be deleted (typically because the event has been detected as being 'dead')
         */
        public async Task RequestDeletionOfEventAtServer(int eventToBeDeletedId)
        {
            var path = _serverBaseAddress + "/workflows/" + _workflowId + "/" + eventToBeDeletedId;
            try
            { 
                await _httpClient.Delete(path);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

    }
}