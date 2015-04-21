using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using Common.Exceptions;

namespace Client.Connections
{
    public class ServerConnection : IServerConnection
    {
        private readonly HttpClientToolbox _http;


        public ServerConnection(Uri uri)
            : this(new HttpClientToolbox(uri))
        {

        }

        /// <summary>
        /// For testing purposes (dependency injection of mocked Toolbox).
        /// </summary>
        /// <param name="toolbox"></param>
        public ServerConnection(HttpClientToolbox toolbox)
        {
            _http = toolbox;
        }

        public async Task<RolesOnWorkflowsDto> Login(string username, string password)
        {
            try
            {
                return
                    await
                        _http.Create<LoginDto, RolesOnWorkflowsDto>("login",
                            new LoginDto {Username = username, Password = password});
            }
            catch (UnauthorizedException e)
            {
                throw new LoginFailedException(e);
            }
            catch (NotFoundException e)
            {
                throw new ServerNotFoundException(e);
            }
        }

        public async Task<IList<WorkflowDto>> GetWorkflows()
        {
            try
            {
                return await _http.ReadList<WorkflowDto>("workflows");
            }
            catch (HttpRequestException ex)
            {
                throw new ServerNotFoundException(ex);
            }

        }

        public Task<IList<EventAddressDto>> GetEventsFromWorkflow(WorkflowDto workflow)
        {
            try
            {
                return _http.ReadList<EventAddressDto>(string.Format("workflows/{0}", workflow.Id));
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("400 (Bad Request)"))
                {
                    throw new InvalidWorkflowIdException(ex);
                }
                throw new ServerNotFoundException(ex);
            }
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
