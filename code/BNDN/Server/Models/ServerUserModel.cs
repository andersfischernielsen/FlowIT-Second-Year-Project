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
            ServerRolesModels = new List<ServerRolesModel>();
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ServerRolesModel> ServerRolesModels { get; set; }
    }
}