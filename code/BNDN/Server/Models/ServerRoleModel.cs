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
        
        //Formely named as Role
        [Required]
        [Key]
        public string ID { get; set; }
        [Key]
        public string ServerWorkflowModelID { get; set; }
        [ForeignKey("ServerWorkflowModelID")]
        public ServerWorkflowModel ServerWorkflowModel { get; set; }
        public virtual ICollection<ServerUserModel> ServerUserModels { get; set; }
        public virtual ICollection<ServerEventModel> ServerEventModels { get; set; }
    }
}