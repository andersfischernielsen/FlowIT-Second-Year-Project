using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Event.Models
{
    public class EventStateModel
    {
        [Key]
        public int Id { get; set; }
        public bool Executed { get; set; }
        public bool Included { get; set; }
        public bool Pending { get; set; }

        public LockDto LockDto { get; set; }
    }
}