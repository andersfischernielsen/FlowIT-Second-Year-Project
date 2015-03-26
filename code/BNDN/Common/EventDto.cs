using System;
using System.Collections.Generic;

namespace Common
{
    public class EventDto
    {
        public string EventId { get; set; }
        public string WorkflowId { get; set; }
        public string Name { get; set; }
        public bool Pending { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public IEnumerable<Uri> Conditions { get; set; }
        public IEnumerable<Uri> Exclusions { get; set; }
        public IEnumerable<Uri> Responses { get; set; }
        public IEnumerable<Uri> Inclusions { get; set; }
    }
}
