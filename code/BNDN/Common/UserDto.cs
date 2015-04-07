using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<WorkflowRole> Roles { get; set; }
    }

    public class WorkflowRole
    {
        public string Role { get; set; }
        public string Workflow { get; set; }
    }
}
