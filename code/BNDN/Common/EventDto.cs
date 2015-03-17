using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class EventDto
    {
        public string Id { get; set; }
        public bool Pending { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public List<IPEndPoint> Conditions { get; set; }
        public List<IPEndPoint> Exclusions { get; set; }
        public List<IPEndPoint> Responses { get; set; }
        public List<IPEndPoint> Inclusions { get; set; }
    }
}
