using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using System.Web;
using Event.Models;

namespace Event
{
    public class LockDto
    {
        [Key, ForeignKey("EventIdentificationModel")]
        // Used for database purposes
        public string Id { get; set; }
        public virtual EventIdentificationModel EventIdentificationModel { get; set; }

        //It's expected that LockOwner matches the Id of the EventAddressDto making the lock call.
        public string LockOwner { get; set; }
        
        
    }
}