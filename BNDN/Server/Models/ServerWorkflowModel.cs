using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerWorkflowModel
    {
        [Required, Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ServerEventModel> ServerEventModels { get; set; }

        public virtual ICollection<ServerRoleModel> ServerRolesModels { get; set; }
    }
}