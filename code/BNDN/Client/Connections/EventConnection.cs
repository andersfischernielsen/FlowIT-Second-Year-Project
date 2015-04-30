using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Client.Exceptions;
using Client.ViewModels;
using Common;
using Common.DTO.Event;
using Common.DTO.History;
using Common.Exceptions;
using Common.Tools;

namespace Client.Connections
{
    public class EventConnection : IEventConnection
    {
        private readonly HttpClientToolbox _httpClient;

        /// <summary>
        /// This constructor is used forwhen the connection should have knowlegde about roles.
        /// </summary>
        /// <param name="eventUri"></param>
        public EventConnection(Uri eventUri)
        {
            _httpClient = new HttpClientToolbox(eventUri);
        }


        /// <summary>
        /// For testing purposes (dependency injection of mocked Toolbox).
        /// </summary>
        /// <param name="toolbox"></param>
        public EventConnection(HttpClientToolbox toolbox)
        {
            _httpClient = toolbox;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotFoundException">If the resource isn't found</exception>
        /// <exception cref="LockedException">If an event is locked</exception>
        /// <exception cref="HostNotFoundException">If the host wasn't found.</exception>
        /// <exception cref="Exception">If an unexpected error happened</exception>
        public async Task<EventStateDto> GetState(string workflowId, string eventId)
        {
            try
            {
                return
                    await _httpClient.Read<EventStateDto>(string.Format("events/{0}/{1}/state/-1", workflowId, eventId));
            }
            catch (HttpRequestException e)
            {
                throw new HostNotFoundException(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotFoundException">If the resource isn't found</exception>
        /// <exception cref="HostNotFoundException">If the host wasn't found.</exception>
        /// <exception cref="Exception">If an unexpected error happened</exception>
        public async Task<IEnumerable<HistoryDto>> GetHistory(string workflowId, string eventId)
        {
            try
            {
                return await _httpClient.ReadList<HistoryDto>(string.Format("history/{0}/{1}", workflowId, eventId));
            }
            catch (HttpRequestException e)
            {
                throw new HostNotFoundException(e);
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotFoundException">If the resource isn't found</exception>
        /// <exception cref="HostNotFoundException">If the host wasn't found.</exception>
        /// <exception cref="Exception">If an unexpected error happened</exception>
        public async Task ResetEvent(string workflowId, string eventId)
        {
            try
            {
                await
                    _httpClient.Update(string.Format("events/{0}/{1}/reset", workflowId, eventId), (object) null);
            }
            catch (HttpRequestException e)
            {
                throw new HostNotFoundException(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotFoundException">If the resource isn't found</exception>
        /// <exception cref="UnauthorizedException">If the user does not have the right access rights</exception>
        /// <exception cref="LockedException">If an event is locked</exception>
        /// <exception cref="NotExecutableException">If an event is not executable, when execute is pressed</exception>
        /// <exception cref="HostNotFoundException">If the host wasn't found.</exception>
        /// <exception cref="Exception">If an unexpected error happened</exception>
        public async Task Execute(string workflowId, string eventId)
        {
            IList<string> roles;
            LoginViewModel.RolesForWorkflows.TryGetValue(workflowId, out roles);
            try
            {
                await
                    _httpClient.Update(string.Format("events/{0}/{1}/executed/", workflowId, eventId),
                        new RoleDto {Roles = roles});
            }
            catch (HttpRequestException e)
            {
                throw new HostNotFoundException(e);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
