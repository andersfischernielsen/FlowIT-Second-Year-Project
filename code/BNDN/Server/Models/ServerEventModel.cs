using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerEventModel
    {
        public string EventId { get; set; }
        public Uri Uri { get; set; }


        public string ServerWorkflowModelId { get; set; }
        public virtual ServerWorkflowModel ServerWorkflowModel { get; set; }
    }
}