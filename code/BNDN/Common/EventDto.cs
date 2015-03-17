using System.Collections.Generic;
using System.Net;

namespace Common
{
    public class EventDto
    {
        public string Id { get; set; }
        public bool Pending { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public IEnumerable<IPEndPoint> Conditions { get; set; }
        public IEnumerable<IPEndPoint> Exclusions { get; set; }
        public IEnumerable<IPEndPoint> Responses { get; set; }
        public IEnumerable<IPEndPoint> Inclusions { get; set; }
    }
}
