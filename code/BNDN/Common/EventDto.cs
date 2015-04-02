using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    // TODO: Discuss: Currently only 2 fields are [Required] - is that sufficient?
    public class EventDto
    {
        [Required]
        public string EventId { get; set; }
        [Required]
        public string WorkflowId { get; set; }
        public string Name { get; set; }
        public bool Pending { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public string Role { get; set; }
        public IEnumerable<Uri> Conditions { get; set; }
        public IEnumerable<Uri> Exclusions { get; set; }
        public IEnumerable<Uri> Responses { get; set; }
        public IEnumerable<Uri> Inclusions { get; set; }
    }
}
