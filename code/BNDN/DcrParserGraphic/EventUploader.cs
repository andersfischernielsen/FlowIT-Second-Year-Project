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
        public async Task Upload(IList<EventDto> events)
        {
            var tool = new HttpClientToolbox("http://localhost:13752/");
            foreach (var e in events)
            {
                await tool.Create("events", e);
            }
        }
    }
}
