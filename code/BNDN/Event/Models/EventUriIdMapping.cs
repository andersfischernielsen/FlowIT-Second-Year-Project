using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace Event.Models
{
    public class EventUriIdMapping
    {
        public Uri Uri { get; set; }
        public string Id { get; set; }
    }
}