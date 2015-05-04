﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Client.Exceptions;
using Client.ViewModels;
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
        public EventConnection()
        {
            _httpClient = new HttpClientToolbox();
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
        public async Task<EventStateDto> GetState(Uri uri, string workflowId, string eventId)
        {
            try
            {
                return
                    await _httpClient.Read<EventStateDto>(string.Format("{0}events/{1}/{2}/state/-1", uri, workflowId, eventId));
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
        public async Task<IEnumerable<HistoryDto>> GetHistory(Uri uri, string workflowId, string eventId)
        {
            try
            {
                return await _httpClient.ReadList<HistoryDto>(string.Format("{0}history/{1}/{2}", uri, workflowId, eventId));
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
        public async Task ResetEvent(Uri uri, string workflowId, string eventId)
        {
            try
            {
                await
                    _httpClient.Update(string.Format("{0}events/{1}/{2}/reset", uri, workflowId, eventId), (object) null);
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
        public async Task Execute(Uri uri, string workflowId, string eventId)
        {
            ICollection<string> roles;
            LoginViewModel.RolesForWorkflows.TryGetValue(workflowId, out roles);
            try
            {
                await
                    _httpClient.Update(string.Format("{0}events/{1}/{2}/executed/", uri, workflowId, eventId),
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
