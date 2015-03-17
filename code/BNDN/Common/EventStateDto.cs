using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class EventStateDto
    {
        public bool Pending { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Executable { get; set; }
    }
}
