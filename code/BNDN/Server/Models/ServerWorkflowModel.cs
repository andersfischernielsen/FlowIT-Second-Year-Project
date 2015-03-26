using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerWorkflowModel
    {
        public string WorkflowId { get; set; }
        public string Name { get; set; }

        public virtual IList<ServerEventModel> ServerEventModels { get; set; }
    }
}