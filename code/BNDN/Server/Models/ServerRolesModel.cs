using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerRolesModel
    {
        
        //Formely named as Role
        [Required]
        public string ID { get; set; }
        public string ServerWorklowModelID { get; set; }
        public virtual ServerWorkflowModel ServerWorkflowModel { get; set; }
        public int ServerUserModelID { get; set; }
        public virtual ServerUserModel ServerUserModel { get; set; }
    }
}