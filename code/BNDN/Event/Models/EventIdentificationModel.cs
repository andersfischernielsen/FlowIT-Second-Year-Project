using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Event.Models
{
    public class EventIdentificationModel
    {
        [Key]
        public int Id { get; set; }
        public string OwnUri { get; set; }
        public string WorkflowId { get; set; }
        public string EventId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }
}