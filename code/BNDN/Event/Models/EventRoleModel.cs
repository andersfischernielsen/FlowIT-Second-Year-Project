using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Event.Models
{
    public class EventRoleModel
    {
        [Key, Column(Order = 0)]
        public string EventId { get; set; }
        [ForeignKey("EventId")]
        public EventIdentificationModel Event { get; set; }
        [Key, Column(Order = 1)]
        public string Role { get; set; }
    }
}