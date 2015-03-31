using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public string ID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ServerEventModel> ServerEventModels { get; set; }

        public virtual ICollection<ServerRolesModel> ServerRolesModels { get; set; }
    }
}