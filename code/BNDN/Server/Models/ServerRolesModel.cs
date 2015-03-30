using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerRolesModel
    {
        public string Role { get; set; }
        public string WorklowId { get; set; }
        public virtual ServerWorkflowModel Worklow { get; set; }
        public int UserId { get; set; }
        public virtual ServerUserModel User { get; set; }
    }
}