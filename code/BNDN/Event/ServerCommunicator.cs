using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Common;
using Event.Controllers;
using HttpClientToolBox;

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
            _serverBaseAddress = "//localhost/api";
            _httpClient = new AwiaHttpClientToolbox("testAdress");
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
                var list = await _httpClient.ReadList<EventAddressDto>(_serverBaseAddress + "/Workflows/" + workflowId);
                return list;
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
        public async Task<int> SendHeartbeatToServer()
        {
            string path = _serverBaseAddress + "/" + _workflowId + "/" + _eventId;
            var heartBeatDto = new HeartBeatDto().EventId = _eventId;
            try
            {
                var result = await _httpClient.Create(path, heartBeatDto);
                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }


        /*
         * SubmitMyselfToServer will inform Server, that this Event wants to join a given workflow
         */

        // TODO: Decide on return-type should be of type Task<...>
        // TODO: Discuss what information is needed for Server to 'enroll' an event into a workflow? 
        public void SubmitMyselfToServer()
        {
            var path = _serverBaseAddress + "/" + _workflowId + "/";

            // TODO:  For now, the following is a 'dummy' object; it only represents that we eventually will have to send some info about this event along to Server     
            var infoToServerAboutThisEvent = "" + _eventId;

            var result = _httpClient.Create(path, infoToServerAboutThisEvent);
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
                // TODO: Ask Wind why Delete is of generic type a.k.a why the <>s in Delete<>() ...?
                // Do not care about the <string> - see line above! 
                await _httpClient.Delete<string>(path);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

    }
}