﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class ServerUserModel
    {
        [Key]
        public string Name { get; set; }
        public string Password { get; set; }

        public virtual ICollection<ServerRoleModel> ServerRolesModels { get; set; }
    }
}