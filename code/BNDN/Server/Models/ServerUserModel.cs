using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerUserModel
    {
        public ServerUserModel()
        {
            ServerRolesModels = new List<ServerRoleModel>();
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ServerRoleModel> ServerRolesModels { get; set; }
    }
}