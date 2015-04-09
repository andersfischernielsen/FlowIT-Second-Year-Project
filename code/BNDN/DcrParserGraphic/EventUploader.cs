using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace DcrParserGraphic
{
    public class EventUploader
    {
        private readonly string _workflow;
        private readonly string _serverAddress, _eventAddress;
        public EventUploader(string workflow, string serverAddress, string eventAddress)
        {
            _workflow = workflow;
            _serverAddress = serverAddress;
            _eventAddress = eventAddress;
        }

        public async Task CreateWorkflow(string workflowDescription)
        {
            var tool = new HttpClientToolbox(_serverAddress);
            await tool.Create("workflows", new WorkflowDto { Id = _workflow, Name = workflowDescription });
        }

        public async Task Upload(IList<EventDto> events)
        {
            var tool = new HttpClientToolbox(_eventAddress);
            foreach (var e in events)
            {
                await tool.Create("events", e);
            }
        }
        //THIS MUST HAPPEN after Upload()
        public async Task UploadUsers(IEnumerable<string> roles)
        {
            var tool = new HttpClientToolbox(_serverAddress);
            foreach (var user in roles.Select(r => new UserDto
            {
                Name = r, 
                Roles = new List<WorkflowRole>
                {
                    new WorkflowRole
                    {
                        Role = r, 
                        Workflow = _workflow
                    }
                }
                
            }))
            {
                await tool.Create("login", user);
            }
        }
    }
}
