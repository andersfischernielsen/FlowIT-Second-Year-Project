using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerRoleModel
    {
        [Key][Column(Order = 0)]
        public string ID { get; set; }
        [Key][Column(Order = 1)]
        public string ServerWorkflowModelID { get; set; }
        [ForeignKey("ServerWorkflowModelID")]
        public ServerWorkflowModel ServerWorkflowModel { get; set; }
        public virtual ICollection<ServerUserModel> ServerUserModels { get; set; }
        public virtual ICollection<ServerEventModel> ServerEventModels { get; set; }
    }
}