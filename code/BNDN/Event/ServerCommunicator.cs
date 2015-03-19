using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Common;
using Event.Controllers;

namespace Event
{

    /*
     * ServerCommunicator is the module through which Event communicates with Server 
     * ServerCommunicator implements the IServerFromEvent interface
     */
    public class ServerCommunicator : IServerFromEvent
    {
        private AwiaHttpClientToolbox _httpClient;
        private string _serverBaseAddress;
        
        // _eventId represents this Event's id
        private int _eventId;
        private int _workflowId;

        public ServerCommunicator(String baseAddress, int eventId, int workFlowId)
        {
            _workflowId = workFlowId;
             _eventId = eventId;
            _serverBaseAddress = baseAddress;
            _httpClient = new AwiaHttpClientToolbox(_serverBaseAddress);
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
                var a = await _httpClient.ReadList<EventAddressDto>("Workflows/" + workflowId);
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


        /*
         * SubmitMyselfToServer will inform Server, that this Event wants to join a given workflow
         */
        // TODO: Discuss what information is needed for Server to 'enroll' an event into a workflow?
        // TODO: Exception handling
        public async void SubmitMyselfToServer()
        {
            // TODO: When posting myself to Server, at what address ("path") should I do that? 
            var path = _workflowId + "/";

            // TODO:  For now, the following is a 'dummy' object; it only represents that we eventually will have to send some info about this event along to Server     
            // DTO should (probably) include id + name of event, and address
            var infoToServerAboutThisEvent = "" + _eventId;

            await _httpClient.Create<String>(path, infoToServerAboutThisEvent);
        }


        /**
         * RequestDeletionOfEventAtServer will inform Server that an event with id eventToBeDeletedId
         * should be deleted (typically because the event has been detected as being 'dead')
         */
        public async Task RequestDeletionOfEventAtServer(int eventToBeDeletedId)
        {
            var path = _serverBaseAddress + "/" + _workflowId + "/" + eventToBeDeletedId;
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