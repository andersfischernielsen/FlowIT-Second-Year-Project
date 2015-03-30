using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerEventModel
    {
        [Key]
        public string EventId { get; set; }
        public string Uri { get; set; }


        public string ServerWorkflowModelId { get; set; }

        public virtual ServerWorkflowModel ServerWorkflowModel { get; set; }
    }
}