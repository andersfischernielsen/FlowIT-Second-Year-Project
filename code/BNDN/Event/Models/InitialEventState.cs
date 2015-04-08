using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Event.Models
{
    /// <summary>
    /// InitialEventState stores bool values an Event was initially created with
    /// </summary>
    public class InitialEventState
    {
        [Key]
        public string EventId { get; set; }
        public bool Pending { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
    }
}