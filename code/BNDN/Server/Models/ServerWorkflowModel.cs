﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerWorkflowModel
    {
        public ServerWorkflowModel()
        {
            ServerRolesModels = new List<ServerRolesModel>();
            ServerEventModels = new List<ServerEventModel>();
        }

        public string WorkflowId { get; set; }
        public string Name { get; set; }

        public virtual IList<ServerEventModel> ServerEventModels { get; set; }

        public virtual IList<ServerRolesModel> ServerRolesModels { get; set; }
    }
}