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
        public EventUploader(string workflow)
        {
            _workflow = workflow;
        }

        public async Task CreateWorkflow(string workflowDescription)
        {
            var tool = new HttpClientToolbox("http://localhost:13768");
            await tool.Create("workflows", new WorkflowDto { Id = _workflow, Name = workflowDescription });
        }

        public async Task Upload(IList<EventDto> events)
        {
            var tool = new HttpClientToolbox("http://localhost:13752/");
            foreach (var e in events)
            {
                await tool.Create("events", e);
            }
        }
        //THIS MUST HAPPEN after Upload()
        public async Task UploadUsers(IEnumerable<string> roles)
        {
            var tool = new HttpClientToolbox("http://localhost:13768/");
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
