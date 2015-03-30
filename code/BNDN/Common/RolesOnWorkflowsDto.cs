using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class RolesOnWorkflowsDto
    {
        public RolesOnWorkflowsDto()
        {
            RolesOnWorkflows = new Dictionary<string, IList<string>>();
        }
        public Dictionary<string, IList<string>> RolesOnWorkflows { get; set; }  // Key is workflowId, value is list of roles on that workflow.
    }
}
