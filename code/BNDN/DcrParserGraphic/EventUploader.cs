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
            var tool = new HttpClientToolbox("http://localhost:13752/");
            foreach (var r in roles)
            {
                var user = new UserDto()
                {
                    Name = r, 
                    Roles = new List<WorkflowRole>()
                    {
                        new WorkflowRole()
                        {
                            Role = r, 
                            Workflow = _workflow
                        }
                    }
                
                };
                await tool.Create("login", user);
            }
        }
    }
}
