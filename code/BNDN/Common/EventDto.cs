using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<EventAddressDto> Conditions { get; set; }
        public List<EventAddressDto> Exclusions { get; set; }
        public List<EventAddressDto> Responses { get; set; }
        public List<EventAddressDto> Inclusions { get; set; }
    }
}
