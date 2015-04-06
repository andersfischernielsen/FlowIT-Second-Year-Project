using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerEventModel
    {
        [Required]
        public string ID { get; set; }
        [Required]
        public string Uri { get; set; }

        [Required]
        public string ServerWorkflowModelID { get; set; }
        [Required]
        public virtual ServerWorkflowModel ServerWorkflowModel { get; set; }
    }
}