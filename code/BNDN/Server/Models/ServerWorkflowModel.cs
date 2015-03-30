using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Key]
        public string WorkflowId { get; set; }
        public string Name { get; set; }

        [ForeignKey("EventId")]
        public virtual IList<ServerEventModel> ServerEventModels { get; set; }

        public virtual IList<ServerRolesModel> ServerRolesModels { get; set; }
    }
}