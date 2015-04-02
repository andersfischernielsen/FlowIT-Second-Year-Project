using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerRolesModel
    {
        //Formely named as Role
        public string ID { get; set; }
        public string ServerWorklowModelID { get; set; }
        public virtual ServerWorkflowModel ServerWorkflowModel { get; set; }
        public virtual ICollection<ServerUserModel> ServerUserModels { get; set; }
    }
}