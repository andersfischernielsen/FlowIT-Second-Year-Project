using System.Collections.Generic;

namespace Common.DTO.Server
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
